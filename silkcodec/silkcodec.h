#ifndef SILK_CODEC_H
#define SILK_CODEC_H

#include "SKP_Silk_typedef.h"

#ifdef _WIN32
    #define SILK_DLL_EXPORT __declspec(dllexport)
#else
    #define SILK_DLL_EXPORT
#endif

/* Define codec specific settings should be moved to h file */
#define DEC_MAX_BYTES_PER_FRAME     1024
#define DEC_MAX_INPUT_FRAMES        5
#define DEC_MAX_FRAME_LENGTH        480
#define DEC_FRAME_LENGTH_MS         20
#define DEC_MAX_API_FS_KHZ          48
#define DEC_MAX_LBRR_DELAY          2

#define SILK_DEC_OK 0
#define SILK_DEC_FILENOTFOUND 1
#define SILK_DEC_WRONGHEADER 2
#define SILK_DEC_OUTPUTNOTFOUND 3
#define SILK_DEC_DECERROR 4
#define SILK_DEC_NULLINPUTSTREAM 5
#define SILK_DEC_NULLOUTPUTSTREAM 6

extern SILK_DLL_EXPORT SKP_int32 silk_decode(char* slk, SKP_uint64 length, char** pcm, SKP_uint64* outlen, SKP_int32 ar, SKP_float loss);
extern SILK_DLL_EXPORT SKP_int32 silk_decode_file( char* inputfile, char* outputfile, SKP_int32 ar, SKP_float loss);

/* Define codec specific settings */
#define ENC_MAX_BYTES_PER_FRAME     250 // Equals peak bitrate of 100 kbps 
#define ENC_MAX_INPUT_FRAMES        5
#define ENC_FRAME_LENGTH_MS         20
#define ENC_MAX_API_FS_KHZ          48

#define SILK_ENC_OK 0
#define SILK_ENC_INPUTNOTFOUND 1
#define SILK_ENC_OUTPUTNOTFOUND 2
#define SILK_ENC_CREATCODERERROR 3
#define SILK_ENC_RESETCODERERROR 4
#define SILK_ENC_SAMPLERATEOUTOFRANGE 5
#define SILK_ENC_NULLINPUTSTREAM 6
#define SILK_ENC_NULLOUTPUTSTREAM 7

extern SILK_DLL_EXPORT SKP_int32 silk_encode_file( char* inputfile, char* outputfile, SKP_int32 Fs_API, SKP_int32 rate, SKP_int32 packetlength, SKP_int32 complecity,
                    SKP_int32 intencent, SKP_int32 loss, SKP_int32 dtx, SKP_int32 inbandfec, SKP_int32 Fs_maxInternal);
extern SILK_DLL_EXPORT SKP_int32 silk_encode( char* pcm, SKP_uint64 length, char** slk, SKP_uint64* outlen, SKP_int32 Fs_API, SKP_int32 rate, SKP_int32 packetlength, SKP_int32 complecity,
                    SKP_int32 intencent, SKP_int32 loss, SKP_int32 dtx, SKP_int32 inbandfec, SKP_int32 Fs_maxInternal);

#endif //SILK_CODEC_H