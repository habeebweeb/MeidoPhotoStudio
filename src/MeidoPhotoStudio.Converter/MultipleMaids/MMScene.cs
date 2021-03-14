namespace MeidoPhotoStudio.Converter.MultipleMaids
{
    public class MMScene
    {
        public readonly string Data;
        public readonly string? ScreenshotBase64;

        public MMScene(string data, string? screenshotBase64)
        {
            Data = data;
            ScreenshotBase64 = screenshotBase64;
        }
    }
}
