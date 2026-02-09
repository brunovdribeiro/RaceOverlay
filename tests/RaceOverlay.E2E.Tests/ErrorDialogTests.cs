namespace RaceOverlay.E2E.Tests;

/// <summary>
/// Error dialog tests are limited in E2E because we cannot easily trigger
/// unhandled exceptions from outside the process.
/// </summary>
[Collection("App")]
public class ErrorDialogTests
{
    private readonly AppFixture _fixture;

    public ErrorDialogTests(AppFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Cannot trigger unhandled exception from E2E — requires app test-mode hook")]
    public void ErrorDialog_ShouldShowOnUnhandledException() { }

    [Fact(Skip = "Cannot trigger unhandled exception from E2E — requires app test-mode hook")]
    public void ErrorDialog_CopyButton_CopiesToClipboard() { }

    [Fact(Skip = "Cannot trigger unhandled exception from E2E — requires app test-mode hook")]
    public void ErrorDialog_CloseButton_DismissesDialog() { }

    [Fact(Skip = "Cannot trigger unhandled exception from E2E — requires app test-mode hook")]
    public void ErrorDialog_ShouldDisplayExceptionDetails() { }
}
