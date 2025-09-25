namespace DiscRipper
{
    internal static class RipperUtils
    {
        #region Feedback window

        public static async Task<string> ScanDrives()
        {
            return await ScanCommon(runner => runner.Drives());
        }

        public static async Task<string> ScanDisc(MakeMkv.Drive drive)
        {
            return await ScanCommon(runner => runner.Info(drive.Index));
        }

        private static async Task<string> ScanCommon(Func<MakeMkvRunner, Task> job)
        {
            MakeMkvRunner runner = new();
            Task runnerTask = job(runner);

            //MakeMkvFeedback feedback = new() { Title = "Scanning Drives", Owner = this, MakeMkvRunner = runner };
            MakeMkvFeedback feedback = new() { Title = "Scanning Drives" };
            feedback += runner;
            feedback.Show();

            await runnerTask;

            return runner.StandardOutput;
        }

        #endregion Feedback window

        #region Feedback (usercontrol)

        ///
        public static async Task<string> ScanDrives(Feedback feedback)
        {
            return await ScanCommon(feedback, runner => runner.Drives());
        }

        public static async Task<string> ScanDisc(Feedback feedback, MakeMkv.Drive drive)
        {
            return await ScanCommon(feedback, runner => runner.Info(drive.Index));
        }

        private static async Task<string> ScanCommon(Feedback feedback, Func<MakeMkvRunner, Task> job)
        {
            MakeMkvRunner runner = new();
            Task runnerTask = job(runner);

            feedback += runner;

            await runnerTask;

            return runner.StandardOutput;
        }

        #endregion Feedback (usercontrol)
    }
}
