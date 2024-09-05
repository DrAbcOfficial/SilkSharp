#include "silkcodec.h"

#ifdef _WIN32
#define _CRT_SECURE_NO_DEPRECATE    1
#endif

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "SKP_Silk_SDK_API.h"
#include "SKP_Silk_SigProc_FIX.h"


#ifdef _SYSTEM_IS_BIG_ENDIAN
/* Function to convert a little endian int16 to a */
/* big endian int16 or vica verca                 */
void swap_endian(
    SKP_int16       vec[],
    SKP_int         len
)
{
    SKP_int i;
    SKP_int16 tmp;
    SKP_uint8 *p1, *p2;

    for( i = 0; i < len; i++ ){
        tmp = vec[ i ];
        p1 = (SKP_uint8 *)&vec[ i ]; p2 = (SKP_uint8 *)&tmp;
        p1[ 0 ] = p2[ 1 ]; p1[ 1 ] = p2[ 0 ];
    }
}
#endif

#if (defined(_WIN32) || defined(_WINCE))
#include <windows.h>	/* timer */
#include <share.h>
#include <io.h>
#include <fcntl.h>
#include <sys/stat.h>

#else    // Linux or Mac
#include <sys/time.h>
#endif

#ifdef _WIN32
unsigned long GetHighResolutionTime() /* O: time in usec*/
{
    /* Returns a time counter in microsec	*/
    /* the resolution is platform dependent */
    /* but is typically 1.62 us resolution  */
    LARGE_INTEGER lpPerformanceCount;
    LARGE_INTEGER lpFrequency;
    QueryPerformanceCounter(&lpPerformanceCount);
    QueryPerformanceFrequency(&lpFrequency);
    return (unsigned long)((1000000*(lpPerformanceCount.QuadPart)) / lpFrequency.QuadPart);
}
/* https://github.com/Arryboom/fmemopen_windows  */
//fmemeopen
FILE *fmemopen(void *buf, size_t len, const char *type)
{
	int fd;
	FILE *fp;
	char tp[MAX_PATH - 13];
	char fn[MAX_PATH + 1];
	int * pfd = &fd;
	int retner = -1;
	char tfname[] = "MemTF_";
	if (!GetTempPathA(sizeof(tp), tp))
		return NULL;
	if (!GetTempFileNameA(tp, tfname, 0, fn))
		return NULL;
	retner = _sopen_s(pfd, fn, _O_CREAT | _O_SHORT_LIVED | _O_TEMPORARY | _O_RDWR | _O_BINARY | _O_NOINHERIT, _SH_DENYRW, _S_IREAD | _S_IWRITE);
	if (retner != 0)
		return NULL;
	if (fd == -1)
		return NULL;
	fp = _fdopen(fd, "wb+");
	if (!fp) {
		_close(fd);
		return NULL;
	}
	/*File descriptors passed into _fdopen are owned by the returned FILE * stream.If _fdopen is successful, do not call _close on the file descriptor.Calling fclose on the returned FILE * also closes the file descriptor.*/
	fwrite(buf, len, 1, fp);
	rewind(fp);
	return fp;
}
FILE* open_memstream(char** buf, SKP_uint64* size) {
    FILE* f = tmpfile();
    if (!f) 
        return NULL;

    *buf = NULL;
    *size = 0;
    return f;
}
#else    // Linux or Mac
unsigned long GetHighResolutionTime() /* O: time in usec*/
{
    struct timeval tv;
    gettimeofday(&tv, 0);
    return((tv.tv_sec*1000000)+(tv.tv_usec));
}
#endif // _WIN32

/* Seed for the random number generator, which is used for simulating packet loss */
static SKP_int32 rand_seed = 1;

SKP_int32 silk_decode_internal(FILE* bitInFile, FILE* speechOutFile, SKP_int32 ar, SKP_float loss)
{
    unsigned long tottime, starttime;
    double    filetime;
    size_t    counter;
    SKP_int32 totPackets, i, k;
    SKP_int16 ret, len, tot_len;
    SKP_int16 nBytes;
    SKP_uint8 payload[    DEC_MAX_BYTES_PER_FRAME * DEC_MAX_INPUT_FRAMES * ( DEC_MAX_LBRR_DELAY + 1 ) ];
    SKP_uint8 *payloadEnd = NULL, *payloadToDec = NULL;
    SKP_uint8 FECpayload[ DEC_MAX_BYTES_PER_FRAME * DEC_MAX_INPUT_FRAMES ], *payloadPtr;
    SKP_int16 nBytesFEC;
    SKP_int16 nBytesPerPacket[ DEC_MAX_LBRR_DELAY + 1 ], totBytes;
    SKP_int16 out[ ( ( DEC_FRAME_LENGTH_MS * DEC_MAX_API_FS_KHZ ) << 1 ) * DEC_MAX_INPUT_FRAMES ], *outPtr;
    SKP_int32 packetSize_ms=0, API_Fs_Hz = ar;
    SKP_int32 decSizeBytes;
    void      *psDec;
    SKP_float loss_prob;
    SKP_int32 frames, lost;
    SKP_SILK_SDK_DecControlStruct DecControl;

    loss_prob = loss;

   
    /* Check Silk header */
    {
        char header_buf[ 50 ];
        fread(header_buf, sizeof(char), 1, bitInFile);
        header_buf[ strlen( "" ) ] = '\0'; /* Terminate with a null character */
        if( strcmp( header_buf, "" ) != 0 ) {
           counter = fread( header_buf, sizeof( char ), strlen( "!SILK_V3" ), bitInFile );
           header_buf[ strlen( "!SILK_V3" ) ] = '\0'; /* Terminate with a null character */
           if( strcmp( header_buf, "!SILK_V3" ) != 0 ) {
               /* Non-equal strings */
               return SILK_DEC_WRONGHEADER;
           }
        } else {
           counter = fread( header_buf, sizeof( char ), strlen( "#!SILK_V3" ), bitInFile );
           header_buf[ strlen( "#!SILK_V3" ) ] = '\0'; /* Terminate with a null character */
           if( strcmp( header_buf, "#!SILK_V3" ) != 0 ) {
               /* Non-equal strings */
               return SILK_DEC_WRONGHEADER;
           }
        }
    }

    /* Set the samplingrate that is requested for the output */
    if( API_Fs_Hz == 0 ) {
        DecControl.API_sampleRate = 24000;
    } else {
        DecControl.API_sampleRate = API_Fs_Hz;
    }

    /* Initialize to one frame per packet, for proper concealment before first packet arrives */
    DecControl.framesPerPacket = 1;

    /* Create decoder */
    ret = SKP_Silk_SDK_Get_Decoder_Size( &decSizeBytes );
    psDec = malloc( decSizeBytes );
    /* Reset decoder */
    ret = SKP_Silk_SDK_InitDecoder( psDec );
    totPackets = 0;
    tottime    = 0;
    payloadEnd = payload;

    /* Simulate the jitter buffer holding MAX_FEC_DELAY packets */
    for( i = 0; i < DEC_MAX_LBRR_DELAY; i++ ) {
        /* Read payload size */
        counter = fread( &nBytes, sizeof( SKP_int16 ), 1, bitInFile );
#ifdef _SYSTEM_IS_BIG_ENDIAN
        swap_endian( &nBytes, 1 );
#endif
        /* Read payload */
        counter = fread( payloadEnd, sizeof( SKP_uint8 ), nBytes, bitInFile );

        if( ( SKP_int16 )counter < nBytes ) {
            break;
        }
        nBytesPerPacket[ i ] = nBytes;
        payloadEnd          += nBytes;
        totPackets++;
    }

    while( 1 ) {
        /* Read payload size */
        counter = fread( &nBytes, sizeof( SKP_int16 ), 1, bitInFile );
#ifdef _SYSTEM_IS_BIG_ENDIAN
        swap_endian( &nBytes, 1 );
#endif
        if( nBytes < 0 || counter < 1 ) {
            break;
        }

        /* Read payload */
        counter = fread( payloadEnd, sizeof( SKP_uint8 ), nBytes, bitInFile );
        if( ( SKP_int16 )counter < nBytes ) {
            break;
        }

        /* Simulate losses */
        rand_seed = SKP_RAND( rand_seed );
        if( ( ( ( float )( ( rand_seed >> 16 ) + ( 1 << 15 ) ) ) / 65535.0f >= ( loss_prob / 100.0f ) ) && ( counter > 0 ) ) {
            nBytesPerPacket[ DEC_MAX_LBRR_DELAY ] = nBytes;
            payloadEnd                       += nBytes;
        } else {
            nBytesPerPacket[ DEC_MAX_LBRR_DELAY ] = 0;
        }

        if( nBytesPerPacket[ 0 ] == 0 ) {
            /* Indicate lost packet */
            lost = 1;

            /* Packet loss. Search after FEC in next packets. Should be done in the jitter buffer */
            payloadPtr = payload;
            for( i = 0; i < DEC_MAX_LBRR_DELAY; i++ ) {
                if( nBytesPerPacket[ i + 1 ] > 0 ) {
                    starttime = GetHighResolutionTime();
                    SKP_Silk_SDK_search_for_LBRR( payloadPtr, nBytesPerPacket[ i + 1 ], ( i + 1 ), FECpayload, &nBytesFEC );
                    tottime += GetHighResolutionTime() - starttime;
                    if( nBytesFEC > 0 ) {
                        payloadToDec = FECpayload;
                        nBytes = nBytesFEC;
                        lost = 0;
                        break;
                    }
                }
                payloadPtr += nBytesPerPacket[ i + 1 ];
            }
        } else {
            lost = 0;
            nBytes = nBytesPerPacket[ 0 ];
            payloadToDec = payload;
        }

        /* Silk decoder */
        outPtr = out;
        tot_len = 0;
        starttime = GetHighResolutionTime();

        if( lost == 0 ) {
            /* No Loss: Decode all frames in the packet */
            frames = 0;
            do {
                /* Decode 20 ms */
                ret = SKP_Silk_SDK_Decode( psDec, &DecControl, 0, payloadToDec, nBytes, outPtr, &len );
                frames++;
                outPtr  += len;
                tot_len += len;
                if( frames > DEC_MAX_INPUT_FRAMES ) {
                    /* Hack for corrupt stream that could generate too many frames */
                    outPtr  = out;
                    tot_len = 0;
                    frames  = 0;
                }
                /* Until last 20 ms frame of packet has been decoded */
            } while( DecControl.moreInternalDecoderFrames );
        } else {
            /* Loss: Decode enough frames to cover one packet duration */
            for( i = 0; i < DecControl.framesPerPacket; i++ ) {
                /* Generate 20 ms */
                ret = SKP_Silk_SDK_Decode( psDec, &DecControl, 1, payloadToDec, nBytes, outPtr, &len );
                outPtr  += len;
                tot_len += len;
            }
        }

        packetSize_ms = tot_len / ( DecControl.API_sampleRate / 1000 );
        tottime += GetHighResolutionTime() - starttime;
        totPackets++;

        /* Write output to file */
#ifdef _SYSTEM_IS_BIG_ENDIAN
        swap_endian( out, tot_len );
#endif
        fwrite( out, sizeof( SKP_int16 ), tot_len, speechOutFile );

        /* Update buffer */
        totBytes = 0;
        for( i = 0; i < DEC_MAX_LBRR_DELAY; i++ ) {
            totBytes += nBytesPerPacket[ i + 1 ];
        }
        /* Check if the received totBytes is valid */
        if (totBytes < 0 || totBytes > sizeof(payload))
            return SILK_DEC_DECERROR;
        SKP_memmove( payload, &payload[ nBytesPerPacket[ 0 ] ], totBytes * sizeof( SKP_uint8 ) );
        payloadEnd -= nBytesPerPacket[ 0 ];
        SKP_memmove( nBytesPerPacket, &nBytesPerPacket[ 1 ], DEC_MAX_LBRR_DELAY * sizeof( SKP_int16 ) );
    }

    /* Empty the recieve buffer */
    for( k = 0; k < DEC_MAX_LBRR_DELAY; k++ ) {
        if( nBytesPerPacket[ 0 ] == 0 ) {
            /* Indicate lost packet */
            lost = 1;

            /* Packet loss. Search after FEC in next packets. Should be done in the jitter buffer */
            payloadPtr = payload;
            for( i = 0; i < DEC_MAX_LBRR_DELAY; i++ ) {
                if( nBytesPerPacket[ i + 1 ] > 0 ) {
                    starttime = GetHighResolutionTime();
                    SKP_Silk_SDK_search_for_LBRR( payloadPtr, nBytesPerPacket[ i + 1 ], ( i + 1 ), FECpayload, &nBytesFEC );
                    tottime += GetHighResolutionTime() - starttime;
                    if( nBytesFEC > 0 ) {
                        payloadToDec = FECpayload;
                        nBytes = nBytesFEC;
                        lost = 0;
                        break;
                    }
                }
                payloadPtr += nBytesPerPacket[ i + 1 ];
            }
        } else {
            lost = 0;
            nBytes = nBytesPerPacket[ 0 ];
            payloadToDec = payload;
        }

        /* Silk decoder */
        outPtr  = out;
        tot_len = 0;
        starttime = GetHighResolutionTime();

        if( lost == 0 ) {
            /* No loss: Decode all frames in the packet */
            frames = 0;
            do {
                /* Decode 20 ms */
                ret = SKP_Silk_SDK_Decode( psDec, &DecControl, 0, payloadToDec, nBytes, outPtr, &len );
                frames++;
                outPtr  += len;
                tot_len += len;
                if( frames > DEC_MAX_INPUT_FRAMES ) {
                    /* Hack for corrupt stream that could generate too many frames */
                    outPtr  = out;
                    tot_len = 0;
                    frames  = 0;
                }
            /* Until last 20 ms frame of packet has been decoded */
            } while( DecControl.moreInternalDecoderFrames );
        } else {
            /* Loss: Decode enough frames to cover one packet duration */

            /* Generate 20 ms */
            for( i = 0; i < DecControl.framesPerPacket; i++ ) {
                ret = SKP_Silk_SDK_Decode( psDec, &DecControl, 1, payloadToDec, nBytes, outPtr, &len );
                outPtr  += len;
                tot_len += len;
            }
        }

        packetSize_ms = tot_len / ( DecControl.API_sampleRate / 1000 );
        tottime += GetHighResolutionTime() - starttime;
        totPackets++;

        /* Write output to file */
#ifdef _SYSTEM_IS_BIG_ENDIAN
        swap_endian( out, tot_len );
#endif
        fwrite( out, sizeof( SKP_int16 ), tot_len, speechOutFile );

        /* Update Buffer */
        totBytes = 0;
        for( i = 0; i < DEC_MAX_LBRR_DELAY; i++ ) {
            totBytes += nBytesPerPacket[ i + 1 ];
        }

        /* Check if the received totBytes is valid */
        if (totBytes < 0 || totBytes > sizeof(payload))
            return SILK_DEC_DECERROR;
        
        SKP_memmove( payload, &payload[ nBytesPerPacket[ 0 ] ], totBytes * sizeof( SKP_uint8 ) );
        payloadEnd -= nBytesPerPacket[ 0 ];
        SKP_memmove( nBytesPerPacket, &nBytesPerPacket[ 1 ], DEC_MAX_LBRR_DELAY * sizeof( SKP_int16 ) );
    }

    /* Free decoder */
    free( psDec );
    filetime = totPackets * 1e-3 * packetSize_ms;
    return SILK_DEC_OK;
}

SILK_DLL_EXPORT SKP_int32 silk_decode(char* slk, SKP_uint64 length, char** pcm, SKP_uint64* outlen, SKP_int32 ar, SKP_float loss)
{
    FILE *inputstream = fmemopen(slk, length, "rb");
    if (inputstream == NULL)
        return SILK_DEC_NULLINPUTSTREAM;
    char* outbuf;
    SKP_uint64 size = 0;
    FILE *outputstream = open_memstream(&outbuf, &size);
    if (outputstream == NULL)
        return SILK_DEC_NULLOUTPUTSTREAM;
    int ret = silk_decode_internal(inputstream, outputstream, ar, loss);

#ifdef WIN32
    fseek(outputstream, 0, SEEK_END);
    size = ftell(outputstream); 
    outbuf = (char*)malloc(size + 1);
    fread(outbuf, 1, size, outputstream);
#endif

    *pcm = outbuf;
    *outlen = size;
    /* Close files */
    fclose( outputstream );
    fclose( inputstream );
    return ret;
}
SILK_DLL_EXPORT SKP_int32 silk_decode_file( char* inputfile, char* outputfile, SKP_int32 ar, SKP_float loss)
{
     /* Open files */
    FILE* bitInFile = fopen( inputfile, "rb" );
    if( bitInFile == NULL )
        return SILK_DEC_FILENOTFOUND;
    FILE* speechOutFile = fopen( outputfile, "wb" );
    if( speechOutFile == NULL )
        return SILK_DEC_OUTPUTNOTFOUND;

    int ret =  silk_decode_internal(bitInFile, speechOutFile, ar, loss);
    /* Close files */
    fclose( speechOutFile );
    fclose( bitInFile );
    return ret;
}

SKP_int32 silk_encode_internal( FILE* speechInFile, FILE* bitOutFile, SKP_int32 Fs_API, SKP_int32 rate, SKP_int32 packetlength, SKP_int32 complecity,
                    SKP_int32 intencent, SKP_int32 loss, SKP_int32 dtx, SKP_int32 inbandfec, SKP_int32 Fs_maxInternal)
{
    unsigned long tottime, starttime;
    double    filetime;
    size_t    counter;
    SKP_int32 k, totPackets, totActPackets, ret;
    SKP_int16 nBytes;
    double    sumBytes, sumActBytes, avg_rate, act_rate, nrg;
    SKP_uint8 payload[ ENC_MAX_BYTES_PER_FRAME * ENC_MAX_INPUT_FRAMES ];
    SKP_int16 in[ ENC_FRAME_LENGTH_MS * ENC_MAX_API_FS_KHZ * ENC_MAX_INPUT_FRAMES ];
    SKP_int32 encSizeBytes;
    void      *psEnc;
#ifdef _SYSTEM_IS_BIG_ENDIAN
    SKP_int16 nBytes_LE;
#endif

    /* default settings */
    SKP_int32 API_fs_Hz = Fs_API;
    SKP_int32 max_internal_fs_Hz = Fs_maxInternal;
    SKP_int32 targetRate_bps = rate;
    SKP_int32 smplsSinceLastPacket, packetSize_ms = packetlength;
    SKP_int32 frameSizeReadFromFile_ms = 20;
    SKP_int32 packetLoss_perc = loss;
#if LOW_COMPLEXITY_ONLY
    SKP_int32 complexity_mode = complecity;
#else
    SKP_int32 complexity_mode = complecity;
#endif
    SKP_int32 DTX_enabled = dtx, INBandFEC_enabled = inbandfec, tencent = intencent;
    SKP_SILK_SDK_EncControlStruct encControl; // Struct for input to encoder
    SKP_SILK_SDK_EncControlStruct encStatus;  // Struct for status of encoder

    /* If no max internal is specified, set to minimum of API fs and 24 kHz */
    if( max_internal_fs_Hz == 0 ) {
        max_internal_fs_Hz = 24000;
        if( API_fs_Hz < max_internal_fs_Hz ) {
            max_internal_fs_Hz = API_fs_Hz;
        }
    }
    
    /* Add Silk header to stream */
    {
        if( tencent ) 
        {
	        static const char Tencent_break[] = "";
            fwrite( Tencent_break, sizeof( char ), strlen( Tencent_break ), bitOutFile );
        }
        static const char Silk_header[] = "#!SILK_V3";
        fwrite( Silk_header, sizeof( char ), strlen( Silk_header ), bitOutFile );
    }

    /* Create Encoder */
    ret = SKP_Silk_SDK_Get_Encoder_Size( &encSizeBytes );
    if( ret ) 
        return SILK_ENC_CREATCODERERROR;

    psEnc = malloc( encSizeBytes );

    /* Reset Encoder */
    ret = SKP_Silk_SDK_InitEncoder( psEnc, &encStatus );
    if( ret ) 
        return SILK_ENC_RESETCODERERROR;

    /* Set Encoder parameters */
    encControl.API_sampleRate        = API_fs_Hz;
    encControl.maxInternalSampleRate = max_internal_fs_Hz;
    encControl.packetSize            = ( packetSize_ms * API_fs_Hz ) / 1000;
    encControl.packetLossPercentage  = packetLoss_perc;
    encControl.useInBandFEC          = INBandFEC_enabled;
    encControl.useDTX                = DTX_enabled;
    encControl.complexity            = complexity_mode;
    encControl.bitRate               = ( targetRate_bps > 0 ? targetRate_bps : 0 );

    if( API_fs_Hz > ENC_MAX_API_FS_KHZ * 1000 || API_fs_Hz < 0 ) 
        return SILK_ENC_SAMPLERATEOUTOFRANGE;

    tottime              = 0;
    totPackets           = 0;
    totActPackets        = 0;
    smplsSinceLastPacket = 0;
    sumBytes             = 0.0;
    sumActBytes          = 0.0;
    smplsSinceLastPacket = 0;
    
    while( 1 ) {
        /* Read input from file */
        counter = fread( in, sizeof( SKP_int16 ), ( frameSizeReadFromFile_ms * API_fs_Hz ) / 1000, speechInFile );
#ifdef _SYSTEM_IS_BIG_ENDIAN
        swap_endian( in, counter );
#endif
        if( ( SKP_int )counter < ( ( frameSizeReadFromFile_ms * API_fs_Hz ) / 1000 ) ) {
            break;
        }

        /* max payload size */
        nBytes = ENC_MAX_BYTES_PER_FRAME * ENC_MAX_INPUT_FRAMES;

        starttime = GetHighResolutionTime();

        /* Silk Encoder */
        ret = SKP_Silk_SDK_Encode( psEnc, &encControl, in, (SKP_int16)counter, payload, &nBytes );
        tottime += GetHighResolutionTime() - starttime;

        /* Get packet size */
        packetSize_ms = ( SKP_int )( ( 1000 * ( SKP_int32 )encControl.packetSize ) / encControl.API_sampleRate );

        smplsSinceLastPacket += ( SKP_int )counter;
        
        if( ( ( 1000 * smplsSinceLastPacket ) / API_fs_Hz ) == packetSize_ms ) {
            /* Sends a dummy zero size packet in case of DTX period  */
            /* to make it work with the decoder test program.        */
            /* In practice should be handled by RTP sequence numbers */
            totPackets++;
            sumBytes  += nBytes;
            nrg = 0.0;
            for( k = 0; k < ( SKP_int )counter; k++ ) {
                nrg += in[ k ] * (double)in[ k ];
            }
            if( ( nrg / ( SKP_int )counter ) > 1e3 ) {
                sumActBytes += nBytes;
                totActPackets++;
            }

            /* Write payload size */
#ifdef _SYSTEM_IS_BIG_ENDIAN
            nBytes_LE = nBytes;
            swap_endian( &nBytes_LE, 1 );
            fwrite( &nBytes_LE, sizeof( SKP_int16 ), 1, bitOutFile );
#else
            fwrite( &nBytes, sizeof( SKP_int16 ), 1, bitOutFile );
#endif

            /* Write payload */
            fwrite( payload, sizeof( SKP_uint8 ), nBytes, bitOutFile );

            smplsSinceLastPacket = 0;
        }
    }

    /* Write dummy because it can not end with 0 bytes */
    nBytes = -1;

    /* Write payload size */
    if( !tencent ) {
        fwrite( &nBytes, sizeof( SKP_int16 ), 1, bitOutFile );
    }

    /* Free Encoder */
    free( psEnc );

    fflush( speechInFile );
    return SILK_ENC_OK;
}


SILK_DLL_EXPORT SKP_int32 silk_encode_file( char* inputfile, char* outputfile, SKP_int32 Fs_API, SKP_int32 rate, SKP_int32 packetlength, SKP_int32 complecity,
                    SKP_int32 intencent, SKP_int32 loss, SKP_int32 dtx, SKP_int32 inbandfec, SKP_int32 Fs_maxInternal) 
{
    /* Open files */
    FILE* speechInFile = fopen( inputfile, "rb" );
    if( speechInFile == NULL ) 
        return SILK_ENC_INPUTNOTFOUND;
    FILE* bitOutFile = fopen( outputfile, "wb" );
    if( bitOutFile == NULL ) 
        return SILK_ENC_OUTPUTNOTFOUND;

    int ret = silk_encode_internal(speechInFile, bitOutFile, Fs_API, rate, packetlength, complecity, intencent, loss, dtx, inbandfec, Fs_maxInternal);
    fclose( bitOutFile );
    fclose( speechInFile );
    return ret;
}

SILK_DLL_EXPORT SKP_int32 silk_encode( char* pcm, SKP_uint64 length, char** slk, SKP_uint64* outlen, SKP_int32 Fs_API, SKP_int32 rate, SKP_int32 packetlength, SKP_int32 complecity,
                    SKP_int32 intencent, SKP_int32 loss, SKP_int32 dtx, SKP_int32 inbandfec, SKP_int32 Fs_maxInternal) 
{
    FILE *speechInFile = fmemopen(pcm, length, "rb");
    if (speechInFile == NULL)
        return SILK_ENC_NULLINPUTSTREAM;

    char* outbuf;
    SKP_uint64 size = 0;
    FILE *bitOutFile = open_memstream(&outbuf, &size);
    if (bitOutFile == NULL)
        return SILK_ENC_NULLOUTPUTSTREAM;
    int ret = silk_encode_internal(speechInFile, bitOutFile, Fs_API, rate, packetlength, complecity, intencent, loss, dtx, inbandfec, Fs_maxInternal);

#ifdef WIN32
    fseek(bitOutFile, 0, SEEK_END);
    size = ftell(bitOutFile); 
    outbuf = (char*)malloc(size + 1);
    fread(outbuf, 1, size, bitOutFile);
#endif
    
    *slk = outbuf;
    *outlen = size;
    fclose( bitOutFile );
    fclose( speechInFile );
    return ret;
}