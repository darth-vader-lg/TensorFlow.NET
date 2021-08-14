#include "pch.h"

#include <tensorflow/c/tf_tstring.h>

#ifdef TENSORFLOW_EXPORTS
#define CAPI_EXPORT __declspec(dllexport)
#else
#define CAPI_EXPORT __declspec(dllexport)
#endif

#ifdef __cplusplus
extern "C" {
#endif
   
#ifndef TENSORFLOW_FROM_2_5_0   
   CAPI_EXPORT extern void TF_TStringAssignView(TF_TString* dst, const char* src, size_t size)
   {
      TF_TString_AssignView(dst, src, size);
   }
   CAPI_EXPORT extern void TF_StringCopy(TF_TString* dst, const char* src, size_t size)
   {
      TF_TString_Copy(dst, src, size);
   }
   CAPI_EXPORT extern void TF_StringDealloc(TF_TString* str)
   {
      TF_TString_Dealloc(str);
   }
   CAPI_EXPORT extern size_t TF_StringGetCapacity(const TF_TString* str)
   {
      return TF_TString_GetCapacity(str);
   }
   CAPI_EXPORT extern const char* TF_StringGetDataPointer(const TF_TString* str)
   {
      return TF_TString_GetDataPointer(str);
   }
   CAPI_EXPORT extern size_t TF_StringGetSize(const TF_TString* str)
   {
      return TF_TString_GetSize(str);
   }
   CAPI_EXPORT extern TF_TString_Type TF_StringGetType(const TF_TString* str)
   {
      return TF_TString_GetType(str);
   }
   CAPI_EXPORT extern void TF_StringInit(TF_TString* t)
   {
      TF_TString_Init(t);
   }
#endif

#ifdef __cplusplus
}
#endif
