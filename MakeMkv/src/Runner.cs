using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace DiscRipper.MakeMkv;

struct Line
{
    public int Index { get; set; }
    public string Text { get; set; }
}

public class Runner
{
    private Channel<Line>? _channel = null;
    private int _lineCount = 0;
    private Task[]? _consumers = null;

    public required SynchronizationContext SynchronizationContext
    {
        init => Log = new() { SynchronizationContext = value };
    }

    public Log Log { get; private init; }

    public required string MakeMkvDir { get; set; }
    public string MakeMkvConFilepath => Path.Join(MakeMkvDir, "makemkvcon64.exe");

    public async Task Drives()
    {
        await Run($"-r --cache=1 info disc:9999");
    }

    public async Task Info(int driveIndex)
    {
        await Run($"-r --cache=1 info disc:{driveIndex}");
    }

    public async Task Mkv(int driveIndex, int titleIndex, string outputDirectory)
    {
        await Run($"""-r --cache=128 mkv disc:{driveIndex} {titleIndex} "{outputDirectory}" """);
    }

    private async Task Run(string arguments)
    {
        if (!File.Exists(MakeMkvConFilepath))
            return;

        ProcessStartInfo psi = new()
        {
            FileName = MakeMkvConFilepath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };

        // Subscribe using a method instead of a lambda
        process.OutputDataReceived += OnStandardOutReceived;
        process.ErrorDataReceived += OnStandardErrorReceived;

        ConfigureConsumers();

        process.Start();

        // Begin async read
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        await StopConsumers();
    }

    private void OnStandardErrorReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not null)
            Debug.WriteLine(e.Data);
    }

    private async void OnStandardOutReceived(object sender, DataReceivedEventArgs e)
    {
        Debug.Assert(_channel is not null);

        if(e.Data is not null)
            await _channel.Writer.WriteAsync(new Line() { Index = _lineCount++, Text = e.Data });
    }

    public async Task RunDebug(string dummyText)
    {
        string[] lines = dummyText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

        await Parallel.ForAsync<int>(0, lines.Length, (i, ct) =>
        {
            string line = lines[i];

            Debug.WriteLine($"Debug parsing line: {line}");

            Log.Parse(i, line);

            return ValueTask.CompletedTask;
        });
    }

    public async Task RunPreloaded(string fullLogText)
    {
        string[] lines = fullLogText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

        ConfigureConsumers();

        Task[] tasks = new Task[lines.Length];

        for(int i = 0; i < lines.Length; ++i)
        {
            var line = lines[i];
            tasks[i] = _channel.Writer.WriteAsync(new Line() { Index = _lineCount++, Text = line }).AsTask();
        }

        await Task.WhenAll(tasks);

        await StopConsumers();
    }

    [MemberNotNull(nameof(_channel))]
    private void ConfigureConsumers()
    {
        _channel = Channel.CreateUnbounded<Line>();
        _lineCount = 0;

        // Start consumer tasks (parallel processing)
        int consumerCount = Environment.ProcessorCount;

        _consumers = new Task[consumerCount];
        for (int i = 0; i < consumerCount; i++)
        {
            _consumers[i] = Task.Run(async () =>
            {
                await foreach (var line in _channel.Reader.ReadAllAsync())
                {
                    Debug.WriteLine($"Debug parsing line: {line.Text}");

                    Log.Parse(line.Index, line.Text);
                }
            });
        }
    }

    private async Task StopConsumers()
    {
        if (_channel is not null)
        {
            _channel.Writer.Complete();
            await Task.WhenAll(_consumers!);

            _channel = null;
            _consumers = null;
        }
    }
}
