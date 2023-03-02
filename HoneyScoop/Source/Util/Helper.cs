using HoneyScoop.FileHandling;

namespace HoneyScoop.Util;

internal static class Helper {
	/// <summary>
	/// Efficiently converts a byte span into a uint
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	internal static uint FromBigEndian(ReadOnlySpan<byte> bytes) {
		uint res = bytes[0];
		res <<= 8;
		res |= bytes[1];
		res <<= 8;
		res |= bytes[2];
		res <<= 8;
		res |= bytes[3];

		return res;
	}

	/// <summary>
	/// Compares the two arguments and returns an <see cref="AnalysisResult"/> based on whether argument update is a valid update to argument res.<br/><br/>
	/// Valid updates to res are:
	/// <list type="bullet">
	///     <item><see cref="AnalysisResult.Partial"/> (applied if res is <see cref="AnalysisResult.Correct"/>),</item>
	///     <item><see cref="AnalysisResult.FormatError"/> (applied if res is Correct or Partial),</item>
	///     <item><see cref="AnalysisResult.Corrupted"/> (applied if res is Correct, Partial or FormatError),</item>
	/// </list>
	/// </summary>
	/// <param name="res">The base AnalysisResult</param>
	/// <param name="update">The update to the base AnalysisResult</param>
	/// <returns>Argument res, updated by argument update, if valid</returns>
	internal static AnalysisResult UpdateResultWith(this AnalysisResult res, AnalysisResult update) {
		switch(update) {
			case AnalysisResult.Partial:
				if(res == AnalysisResult.Correct) {
					res = AnalysisResult.Partial;
				}
				break;
			case AnalysisResult.FormatError:
				if(res == AnalysisResult.Correct || res == AnalysisResult.Partial) {
					res = AnalysisResult.FormatError;
				}
				break;
			case AnalysisResult.Corrupted:
				if(res == AnalysisResult.Correct || res == AnalysisResult.Partial || res == AnalysisResult.FormatError) {
					res = AnalysisResult.Corrupted;
				}
				break;
		}

		return res;
	}

	private static readonly uint[] CrcTable = MakeCrc32Table();

	/// <summary>
	/// Adapted from <a href="https://www.w3.org/TR/png/#D-CRCAppendix">https://www.w3.org/TR/png/#D-CRCAppendix</a>
	/// </summary>
	/// <returns>The CRC32 table of the CRC32 of each 8-bit value</returns>
	private static uint[] MakeCrc32Table() {
		var table = new uint[256];

		for(int n = 0; n < 256; n++) {
			uint c = (uint)n;
			for(int k = 0; k < 8; k++) {
				if((c & 0x1) == 1) {
					c = 0xedb88320 ^ (c >> 1);
				} else {
					c = c >> 1;
				}
			}

			table[n] = c;
		}

		return table;
	}

	/// <summary>
	/// Adapted from <a href="https://www.w3.org/TR/png/#D-CRCAppendix">https://www.w3.org/TR/png/#D-CRCAppendix</a>
	/// </summary>
	/// <param name="crc">The current CRC32</param>
	/// <param name="buffer">The buffer to update the CRC32 with</param>
	/// <returns>The updated CRC32</returns>
	internal static uint UpdateCrc32(uint crc, ReadOnlySpan<byte> buffer) {
		uint c = crc;

		for(int n = 0; n < buffer.Length; n++) {
			c = CrcTable[(c ^ buffer[n]) & 0xff] ^ (c >> 8);
		}
		return c;
	}

	/// <summary>
	/// Adapted from <a href="https://www.w3.org/TR/png/#D-CRCAppendix">https://www.w3.org/TR/png/#D-CRCAppendix</a>
	/// </summary>
	/// <param name="buffer">The buffer to calculate the CRC32 of</param>
	/// <returns>The CRC32 of the provided buffer</returns>
	internal static uint Crc32(ReadOnlySpan<byte> buffer) {
		return UpdateCrc32(uint.MaxValue, buffer) ^ uint.MaxValue;
	}
}