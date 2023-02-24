using System.Diagnostics;

namespace HoneyScoop.FileHandling;

internal class FileHandler {
	internal const int DefaultBufferSize = 1024 * 1024; // default buffer size in bytes (1 MB)
	
	private readonly string _filePath;
	private readonly int _bufferSize;
	private readonly FileStream _fStream;
	
	/// <summary>
	/// Constructs a new FileHandler object, opening the specified file for processing
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="bufferSize"></param>
	internal FileHandler(string filePath, int bufferSize = DefaultBufferSize) {
		_filePath = filePath;
		_bufferSize = bufferSize;
		_fStream = File.OpenRead(_filePath);
	}

	/// <summary>
	/// Return the next range of bytes as a read only span
	/// </summary>
	/// <returns></returns>
	/// <exception cref="NotImplementedException">Always throws this until implemented</exception>
	internal ReadOnlySpan<byte> Next() {
		throw new NotImplementedException();
	}

	/// <summary>
	/// Must be called once processing is done. Closes the underlying opened file
	/// </summary>
	internal void Close() {
		_fStream.Close();
	}

	// TODO: API change - Instead of having a function in this class (ProcessSection) that processes a section, have a function (Next) that returns the next section/buffer of bytes to the caller
	internal void HandleFile() {
		const int sectionSize = 100; // specify the section size in bytes

		// open the file stream
		using(FileStream stream = File.OpenRead(_filePath)) {
			int bytesRead;
			var buffer = new byte[_bufferSize];
			long totalBytesRead = 0;
			var fileSize = stream.Length; // get the total size of the file
			Stopwatch stopwatch = new Stopwatch(); // create a stopwatch to measure elapsed time

			stopwatch.Start(); // start the stopwatch

			// read the file in chunks and process each section
			while((bytesRead = stream.Read(buffer, 0, _bufferSize)) > 0) {
				// split the chunk into sections and process each section // TODO: Why not just process the entire buffer all at once?
				for(var i = 0; i < bytesRead; i += sectionSize) {
					var sectionBytes = Math.Min(sectionSize, bytesRead - i);
					var section = new byte[sectionBytes];
					Array.Copy(buffer, i, section, 0, sectionBytes);

					// processes the current section
					ProcessSection(section, sectionBytes);
				}

				totalBytesRead += bytesRead; // increment the total bytes read
				var percentage = (int)((double)totalBytesRead / fileSize * 100); // calculate the percentage of file read

				// print the progress to the console
				Console.CursorLeft = 0;
				Console.Write($"Loading: {percentage}% (estimated time remaining: {GetEstimatedTimeRemaining(stopwatch.Elapsed, totalBytesRead, fileSize)})");
			}

			stopwatch.Stop(); // stops the stopwatch
			Console.WriteLine(); // adds a newline to the console
		}

		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine("File has been processed into sections."); // prints a completion message to the console
		Console.ResetColor();
	}
	
	private void ProcessSection(byte[] buffer, int bytesRead) {
		// space for processing the section so probably where our next work will be
	}
	
	private static TimeSpan GetEstimatedTimeRemaining(TimeSpan elapsedTime, long totalBytesRead, long fileSize) {
		var bytesPerSecond = totalBytesRead / elapsedTime.TotalSeconds; // calculate the current speed in bytes per second
		double remainingBytes = fileSize - totalBytesRead; // calculate the remaining bytes to read
		var secondsRemaining = remainingBytes / bytesPerSecond; // calculate the remaining time in seconds

		return TimeSpan.FromSeconds(secondsRemaining); // return the remaining time as a TimeSpan object
	}
}