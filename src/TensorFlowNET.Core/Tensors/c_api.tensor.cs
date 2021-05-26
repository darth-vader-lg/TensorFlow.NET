/*****************************************************************************
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

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tensorflow
{
    public partial class c_api
    {
        /// <summary>
        /// Indicate a Tensorflow version above 2.4.1
        /// </summary>
        private static bool? isAbove2_4_1;

        /// <summary>
        /// Indicate a Tensorflow version above 2.4.1
        /// </summary>
        private static bool IsAbove2_4_1
        {
            get
            {
                if (isAbove2_4_1.HasValue)
                    return isAbove2_4_1.Value;
                var ver = new Version(Binding.tf.VERSION);
                isAbove2_4_1 = ver > new Version("2.4.1");
                return isAbove2_4_1.Value;
            }
        }

        /// <summary>
        /// Allocate and return a new Tensor.
        /// </summary>
        /// <param name="dtype">TF_DataType</param>
        /// <param name="dims">const int64_t*</param>
        /// <param name="num_dims">int</param>
        /// <param name="len">size_t</param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern IntPtr TF_AllocateTensor(TF_DataType dtype, long[] dims, int num_dims, ulong len);

        /// <summary>
        /// returns the sizeof() for the underlying type corresponding to the given TF_DataType enum value.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern ulong TF_DataTypeSize(TF_DataType dt);

        /// <summary>
        /// Destroy a tensor.
        /// </summary>
        /// <param name="tensor"></param>
        [DllImport(TensorFlowLibName)]
        public static extern void TF_DeleteTensor(IntPtr tensor);

        /// <summary>
        /// Return the length of the tensor in the "dim_index" dimension.
        /// REQUIRES: 0 &lt;= dim_index &lt; TF_NumDims(tensor)
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="dim_index"></param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern long TF_Dim(IntPtr tensor, int dim_index);

        /// <summary>
        /// Return a new tensor that holds the bytes data[0,len-1]
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="dims"></param>
        /// <param name="num_dims"></param>
        /// <param name="data"></param>
        /// <param name="len">num_bytes, ex: 6 * sizeof(float)</param>
        /// <param name="deallocator"></param>
        /// <param name="deallocator_arg"></param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern IntPtr TF_NewTensor(TF_DataType dataType, long[] dims, int num_dims, IntPtr data, UIntPtr len, Deallocator deallocator, ref DeallocatorArgs deallocator_arg);

        [DllImport(TensorFlowLibName)]
        public static extern TF_Tensor TF_NewTensor(TF_DataType dataType, long[] dims, int num_dims, IntPtr data, long len, DeallocatorV2 deallocator, IntPtr args);

        /// <summary>
        /// Return a new tensor that holds the bytes data[0,len-1]
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="dims"></param>
        /// <param name="num_dims"></param>
        /// <param name="data"></param>
        /// <param name="len">num_bytes, ex: 6 * sizeof(float)</param>
        /// <param name="deallocator"></param>
        /// <param name="deallocator_arg"></param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern IntPtr TF_NewTensor(TF_DataType dataType, long[] dims, int num_dims, IntPtr data, ulong len, Deallocator deallocator, IntPtr deallocator_arg);

        /// <summary>
        /// Return a new tensor that holds the bytes data[0,len-1]
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="dims"></param>
        /// <param name="num_dims"></param>
        /// <param name="data"></param>
        /// <param name="len">num_bytes, ex: 6 * sizeof(float)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr TF_NewTensor(TF_DataType dataType, long[] dims, int num_dims, IntPtr data, ulong len)
        {
            return c_api.TF_NewTensor(dataType, dims, num_dims, data, len, EmptyDeallocator, DeallocatorArgs.Empty);
        }
        /// <summary>
        /// Return a new tensor that holds the bytes data[0,len-1]
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="dims"></param>
        /// <param name="num_dims"></param>
        /// <param name="data"></param>
        /// <param name="len">num_bytes, ex: 6 * sizeof(float)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr TF_NewTensor(TF_DataType dataType, long[] dims, int num_dims, void* data, ulong len)
        {
            return TF_NewTensor(dataType, dims, num_dims, new IntPtr(data), len);
        }

        /// <summary>
        /// Return the number of dimensions that the tensor has.
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern int TF_NumDims(IntPtr tensor);

        /// <summary>
        /// Return the size of the underlying data in bytes.
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern ulong TF_TensorByteSize(IntPtr tensor);

        /// <summary>
        /// Return a pointer to the underlying data buffer.
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern IntPtr TF_TensorData(IntPtr tensor);

        /// <summary>
        /// Deletes `tensor` and returns a new TF_Tensor with the same content if
        /// possible. Returns nullptr and leaves `tensor` untouched if not.
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern IntPtr TF_TensorMaybeMove(IntPtr tensor);

        /// <summary>
        /// Return the type of a tensor element.
        /// </summary>
        /// <param name="tensor"></param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern TF_DataType TF_TensorType(IntPtr tensor);

        /// <summary>
        /// Return the size in bytes required to encode a string `len` bytes long into a
        /// TF_STRING tensor.
        /// </summary>
        /// <param name="len">size_t</param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern ulong TF_StringEncodedSize(ulong len);

        /// <summary>
        /// Encode the string `src` (`src_len` bytes long) into `dst` in the format
        /// required by TF_STRING tensors. Does not write to memory more than `dst_len`
        /// bytes beyond `*dst`. `dst_len` should be at least
        /// TF_StringEncodedSize(src_len).
        /// </summary>
        /// <param name="src">const char*</param>
        /// <param name="src_len">size_t</param>
        /// <param name="dst">char*</param>
        /// <param name="dst_len">size_t</param>
        /// <param name="status">TF_Status*</param>
        /// <returns>On success returns the size in bytes of the encoded string.</returns>
        [DllImport(TensorFlowLibName)]
        public static extern unsafe ulong TF_StringEncode(byte* src, ulong src_len, byte* dst, ulong dst_len, SafeStatusHandle status);

        [DllImport(TensorFlowLibName, EntryPoint = "TF_StringInit")]
        private static extern void TF_StringInit_Above_241(IntPtr t);

        [DllImport(TensorFlowExportLibName, EntryPoint = "TF_StringInit")]
        private static extern void TF_StringInit_Until_241(IntPtr t);

        public static void TF_StringInit(IntPtr t)
        {
            if (IsAbove2_4_1)
                TF_StringInit_Above_241(t);
            else
                TF_StringInit_Until_241(t);
        }

        [DllImport(TensorFlowLibName, EntryPoint = "TF_StringCopy")]
        public static extern void TF_StringCopy_Above_241(IntPtr dst, byte[] text, long size);

        [DllImport(TensorFlowExportLibName, EntryPoint = "TF_StringCopy")]
        public static extern void TF_StringCopy_Until_241(IntPtr dst, byte[] text, long size);

        public static void TF_StringCopy(IntPtr dst, byte[] text, long size)
        {
            if (IsAbove2_4_1)
                TF_StringCopy_Above_241(dst, text, size);
            else
                TF_StringCopy_Until_241(dst, text, size);
        }

        [DllImport(TensorFlowLibName)]
        public static extern void TF_StringCopy(IntPtr dst, string text, long size);

        [DllImport(TensorFlowLibName, EntryPoint = "TF_StringAssignView")]
        public static extern void TF_StringAssignView_Above_241(IntPtr dst, IntPtr text, long size);

        [DllImport(TensorFlowExportLibName, EntryPoint = "TF_StringAssignView")]
        public static extern void TF_StringAssignView_Until_241(IntPtr dst, IntPtr text, long size);

        public static void TF_StringAssignView(IntPtr dst, IntPtr text, long size)
        {
            if (IsAbove2_4_1)
                TF_StringAssignView_Above_241(dst, text, size);
            else
                TF_StringAssignView_Until_241(dst, text, size);
        }

        [DllImport(TensorFlowLibName, EntryPoint = "TF_StringGetDataPointer")]
        public static extern IntPtr TF_StringGetDataPointer_Above_241(IntPtr tst);

        [DllImport(TensorFlowExportLibName, EntryPoint = "TF_StringGetDataPointer")]
        public static extern IntPtr TF_StringGetDataPointer_Until_241(IntPtr tst);

        public static IntPtr TF_StringGetDataPointer(IntPtr tst)
        {
            return IsAbove2_4_1 ? TF_StringGetDataPointer_Above_241(tst) : TF_StringGetDataPointer_Until_241(tst);
        }

        [DllImport(TensorFlowLibName, EntryPoint = "TF_StringGetType")]
        public static extern TF_TString_Type TF_StringGetType_Above_241(IntPtr tst);

        [DllImport(TensorFlowExportLibName, EntryPoint = "TF_StringGetType")]
        public static extern TF_TString_Type TF_StringGetType_Until_241(IntPtr tst);

        public static TF_TString_Type TF_StringGetType(IntPtr tst)
        {
            return IsAbove2_4_1 ? TF_StringGetType_Above_241(tst) : TF_StringGetType_Until_241(tst);
        }

        [DllImport(TensorFlowLibName, EntryPoint = "TF_StringGetSize")]
        public static extern ulong TF_StringGetSize_Above_241(IntPtr tst);

        [DllImport(TensorFlowExportLibName, EntryPoint = "TF_StringGetSize")]
        public static extern ulong TF_StringGetSize_Until_241(IntPtr tst);

        public static ulong TF_StringGetSize(IntPtr tst)
        {
            return IsAbove2_4_1 ? TF_StringGetSize_Above_241(tst) : TF_StringGetSize_Until_241(tst);
        }

        [DllImport(TensorFlowLibName, EntryPoint = "TF_StringGetCapacity")]
        public static extern ulong TF_StringGetCapacity_Above_241(IntPtr tst);

        [DllImport(TensorFlowExportLibName, EntryPoint = "TF_StringGetCapacity")]
        public static extern ulong TF_StringGetCapacity_Until_241(IntPtr tst);

        public static ulong TF_StringGetCapacity(IntPtr tst)
        {
            return IsAbove2_4_1 ? TF_StringGetCapacity_Above_241(tst) : TF_StringGetCapacity_Until_241(tst);
        }

        [DllImport(TensorFlowLibName, EntryPoint = "TF_StringDealloc")]
        public static extern void TF_StringDealloc_Above_241(IntPtr tst);

        [DllImport(TensorFlowExportLibName, EntryPoint = "TF_StringDealloc")]
        public static extern void TF_StringDealloc_Until_241(IntPtr tst);

        public static void TF_StringDealloc(IntPtr tst)
        {
            if (IsAbove2_4_1)
                TF_StringDealloc_Above_241(tst);
            else
                TF_StringDealloc_Until_241(tst);
        }

        /// <summary>
        /// Decode a string encoded using TF_StringEncode.
        /// </summary>
        /// <param name="src">const char*</param>
        /// <param name="src_len">size_t</param>
        /// <param name="dst">const char**</param>
        /// <param name="dst_len">size_t*</param>
        /// <param name="status">TF_Status*</param>
        /// <returns></returns>
        [DllImport(TensorFlowLibName)]
        public static extern unsafe ulong TF_StringDecode(byte* src, ulong src_len, byte** dst, ref ulong dst_len, SafeStatusHandle status);


        public static c_api.Deallocator EmptyDeallocator = FreeNothingDeallocator;

        [MonoPInvokeCallback(typeof(c_api.Deallocator))]
        private static void FreeNothingDeallocator(IntPtr dataPtr, IntPtr len, ref c_api.DeallocatorArgs args)
        { }

        /// <summary>
        /// This attribute can be applied to callback functions that will be invoked
        /// from unmanaged code to managed code.
        /// </summary>
        /// <remarks>
        /// <code>
        /// [TensorFlow.MonoPInvokeCallback (typeof (BufferReleaseFunc))]
        /// internal static void MyFreeFunc (IntPtr data, IntPtr length){..}
        /// </code>
        /// </remarks>
        public sealed class MonoPInvokeCallbackAttribute : Attribute
        {
            /// <summary>
            /// Use this constructor to annotate the type of the callback function that 
            /// will be invoked from unmanaged code.
            /// </summary>
            /// <param name="t">T.</param>
            public MonoPInvokeCallbackAttribute(Type t) { }
        }
    }
}
