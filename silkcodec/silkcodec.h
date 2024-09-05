#ifndef SILK_CODEC_H
#define SILK_CODEC_H

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

extern SILK_DLL_EXPORT int silk_decode(char* slk, unsigned long long length, char** pcm, unsigned long long* outlen, int ar);
extern SILK_DLL_EXPORT int silk_decode_file( char* inputfile, char* outputfile, int ar );

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

//Fs_API 24000
//rate 25000
//complecity if LOW_COMPLEXITY_ONLY 0 else 2
//Fs_maxInternal 0
//packetlength 20

extern SILK_DLL_EXPORT int silk_encode_file( char* inputfile, char* outputfile, int Fs_API, int rate, int packetlength, int complecity,
                    int intencent, int loss, int dtx, int inbandfec, int Fs_maxInternal);
extern SILK_DLL_EXPORT int silk_encode( char* pcm, unsigned long long length, char** slk, unsigned long long* outlen, int Fs_API, int rate, int packetlength, int complecity,
                    int intencent, int loss, int dtx, int inbandfec, int Fs_maxInternal);

#endif //SILK_CODEC_H