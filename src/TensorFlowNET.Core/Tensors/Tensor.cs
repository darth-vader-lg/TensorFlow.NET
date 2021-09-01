﻿/*****************************************************************************
   Copyright 2018 The TensorFlow.NET Authors. All Rights Reserved.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************/

using Tensorflow.NumPy;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Tensorflow.Eager;
using Tensorflow.Framework;
using Tensorflow.Keras.Engine;
using static Tensorflow.Binding;

namespace Tensorflow
{
    /// <summary>
    /// A tensor is a generalization of vectors and matrices to potentially higher dimensions. 
    /// Internally, TensorFlow represents tensors as n-dimensional arrays of base datatypes.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public partial class Tensor : DisposableObject,
        ITensorOrOperation,
        ITensorOrTensorArray,
        IPackable<Tensor>,
        ICanBeFlattened
    {
        protected long _id;
        private readonly Operation _op;
        private readonly int _value_index;
        private TF_Output? _tf_output;
        private readonly TF_DataType _override_dtype;
        public long Id => _id;

        /// <summary>
        ///     The Graph that contains this tensor.
        /// </summary>
        public Graph graph => op?.graph;

        /// <summary>
        ///     The Operation that produces this tensor as an output.
        /// </summary>
        public Operation op => _op;
        public Tensor[] outputs => op?.outputs;

        /// <summary>
        /// The string name of this tensor.<br/>
        /// Tensor.name is meaningless when eager execution is enabled.
        /// </summary>
        public virtual string name => $"{(op == null ? "<unnamed>" : $"{op.name}:{_value_index}")}";

        /// <summary>
        ///     The index of this tensor in the outputs of its Operation.
        /// </summary>
        public int value_index => _value_index;

        /// <summary>
        ///     The DType of elements in this tensor.
        /// </summary>
        public TF_DataType dtype => _handle == null ? _override_dtype : c_api.TF_TensorType(_handle);
        public ulong bytesize => _handle == null ? 0 : c_api.TF_TensorByteSize(_handle);
        public ulong dtypesize => _handle == null ? 0 : c_api.TF_DataTypeSize(dtype);
        public ulong size => _handle == null ? 0 : bytesize / dtypesize;
        public IntPtr buffer => _handle == null ? IntPtr.Zero : c_api.TF_TensorData(_handle);
        public int num_consumers(TF_Output oper_out) => _handle == null ? 0 : c_api.TF_OperationOutputNumConsumers(oper_out);
        public int ndim => rank;

        /// <summary>
        ///     The name of the device on which this tensor will be produced, or null.
        /// </summary>
        public virtual string Device => op.Device;
        public long[] dims => shape.dims;

        /// <summary>
        ///     Used for keep other pointer when do implicit operating
        /// </summary>
        public object Tag { get; set; }
        protected new SafeTensorHandle _handle;
        public SafeTensorHandle Handle => _handle;

        protected SafeTensorHandleHandle _eagerTensorHandle;
        /// <summary>
        /// TFE_TensorHandle
        /// </summary>
        public SafeTensorHandleHandle EagerTensorHandle => _eagerTensorHandle;

        protected bool isCreatedInGraphMode;
        
        public bool IsCreatedInGraphMode => isCreatedInGraphMode;
        public bool IsSparseTensor => this is SparseTensor;

        public Tensor TensorShape => tf.shape(this);

        /// <summary>
        ///     Returns the shape of a tensor.
        /// </summary>
        /// <remarks>https://www.tensorflow.org/api_docs/python/tf/shape</remarks>
        public Shape shape
        {
            get
            {
                if (rank < 0)
                    return Shape.Null;

                var dims = new Shape(new long[rank]);

                if (_handle == null)
                {
                    c_api.TF_GraphGetTensorShape(op.graph, _as_tf_output(), dims, rank, tf.Status.Handle);
                }
                else
                {
                    for (int i = 0; i < rank; i++)
                        dims[i] = c_api.TF_Dim(_handle, i);
                }

                return dims;
            }

            set
            {
                if (this is EagerTensor)
                {
                    if(!shape.is_compatible_with(value))
                        throw new ValueError($"Tensor's shape is not compatible.");
                    return;
                }

                if (value == null)
                    c_api.TF_GraphSetTensorShape(graph, _as_tf_output(), null, -1, tf.Status.Handle);
                else
                    c_api.TF_GraphSetTensorShape(graph, _as_tf_output(), value.dims, value.ndim, tf.Status.Handle);

                tf.Status.Check(true);
            }
        }

        public int[] _shape_tuple()
        {
            return rank < 0 ? null : shape.dims.Select(x => (int)x).ToArray();
        }

        /// <summary>
        /// Keras History: (Layer, (node_index, tensor_index))
        /// </summary>
        public KerasHistory KerasHistory { get; set; }
        public Tensor KerasMask { get; set; }

        /// <summary>
        ///     Updates the shape of this tensor.
        /// </summary>
        public void set_shape(Tensor shape)
        {
            // ReSharper disable once MergeConditionalExpression
            this.shape = shape is null ? null : shape.shape;
        }

        /// <summary>
        /// number of dimensions <br></br>
        /// -1 Unknown  <br></br>
        /// 0	Scalar (magnitude only) <br></br>
        /// 1	Vector (magnitude and direction) <br></br>
        /// 2	Matrix (table of numbers) <br></br>
        /// 3	3-Tensor (cube of numbers) <br></br>
        /// n	n-Tensor (you get the idea)
        /// </summary>
        /// <remarks>https://www.tensorflow.org/api_docs/python/tf/rank</remarks>
        public virtual int rank
        {
            get
            {
                if (_handle == null)
                {
                    var output = _as_tf_output();
                    int ndim = c_api.TF_GraphGetTensorNumDims(op.graph, output, tf.Status.Handle);
                    return ndim;
                }

                return c_api.TF_NumDims(_handle);
            }
        }

        /// <summary>
        ///     Returns a list of Operations that consume this tensor.
        /// </summary>
        /// <returns></returns>
        public Operation[] consumers()
        {
            var output = _as_tf_output();
            var consumer_names = c_api.TF_OperationOutputConsumers_wrapper(output);
            return consumer_names.Select(x => graph.OperationByName(x)).ToArray();
        }

        public TF_Output _as_tf_output()
        {
            if (!_tf_output.HasValue)
                _tf_output = new TF_Output(op, value_index);

            return _tf_output.Value;
        }
        
        public Tensor MaybeMove()
        {
            var tensor = c_api.TF_TensorMaybeMove(_handle);
            return tensor;
        }

        /// <summary>
        ///     Evaluates this tensor in a `Session`.
        /// </summary>
        /// <param name="feed_dict">A dictionary that maps `Tensor` objects to feed values.</param>
        /// <returns>A <see cref="NumSharp"/> array corresponding to the value of this tensor.</returns>
        public NDArray eval(params FeedItem[] feed_dict)
        {
            return ops._eval_using_default_session(this, feed_dict, graph);
        }

        /// <summary>
        ///     Evaluates this tensor in a `Session`.
        /// </summary>
        /// <param name="feed_dict">A dictionary that maps `Tensor` objects to feed values.</param>
        /// <param name="session">The `Session` to be used to evaluate this tensor.</param>
        /// <returns>A <see cref="NumSharp"/> array corresponding to the value of this tensor.</returns>
        public NDArray eval(Session session, params FeedItem[] feed_dict)
        {
            return ops._eval_using_default_session(this, feed_dict, graph, session);
        }

        public override string ToString()
        {
            // this can throw IndexOutOfRangeException 
            switch (rank)
            {
                case -1:
                    return $"tf.Tensor '{name}' shape={shape} dtype={dtype.as_numpy_name()}";
                case 0:
                    return $"tf.Tensor '{name}' shape={shape} dtype={dtype.as_numpy_name()}";
                default:
                    return $"tf.Tensor '{name}' shape={shape} dtype={dtype.as_numpy_name()}";
            }
        }

        protected override void DisposeUnmanagedResources(IntPtr handle)
        {

        }

        public bool IsDisposed => _disposed;
    }
}