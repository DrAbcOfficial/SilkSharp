cmake_minimum_required(VERSION 3.21)
project(silk-codec VERSION 1.0.0)

set(CMAKE_C_STANDARD 99)
set(CMAKE_C_STANDARD_REQUIRED True)

# silk
include_directories(interface)
include_directories(libsilk)

add_library(silkcodec SHARED silkcodec.c)

add_subdirectory(libsilk)

target_link_libraries(silkcodec PUBLIC SKP_Silk_SDK)