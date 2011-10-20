LOCAL_PATH := $(call my-dir)

#
# FMOD Ex Shared Library
# 
include $(CLEAR_VARS)

LOCAL_MODULE            := fmodex
LOCAL_SRC_FILES         := ../../../FMOD/lib/android-9/$(TARGET_ARCH_ABI)/libfmodex.so
LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/../../../FMOD/inc

include $(PREBUILT_SHARED_LIBRARY)

#
# MJApp
#
include $(CLEAR_VARS)

LOCAL_MODULE := mjapp

LOCAL_SRC_FILES += ../../MJApp.c
LOCAL_SRC_FILES += ../../../Crender/example/Common.c
LOCAL_SRC_FILES += ../../../Crender/example/Framework.android.c
LOCAL_SRC_FILES += ../../../Crender/example/Material.c
LOCAL_SRC_FILES += ../../../Crender/example/Mesh.c
LOCAL_SRC_FILES += ../../../Crender/example/Mesh.obj.c
LOCAL_SRC_FILES += ../../../Crender/example/Pvr.c
LOCAL_SRC_FILES += ../../../Crender/example/Stream.c

LOCAL_STATIC_LIBRARIES := crender android_native_app_glue
#LOCAL_SHARED_LIBRARIES := fmodex

include $(BUILD_SHARED_LIBRARY)

$(call import-module, android/native_app_glue)
$(call import-module, lib)