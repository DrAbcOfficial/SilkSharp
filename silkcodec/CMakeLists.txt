cmake_minimum_required(VERSION 3.16)
project(silk-codec VERSION 1.0.0)

set(CMAKE_C_STANDARD 99)
set(CMAKE_C_STANDARD_REQUIRED True)

# silk
include_directories(interface)
include_directories(libsilk)

aux_source_directory(libsilk SKP_Silk_SDK)

add_library(silkcodec SHARED silkcodec.c ${SKP_Silk_SDK})