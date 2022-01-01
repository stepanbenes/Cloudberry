using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;

namespace Cloudberry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
	[HttpPost("reboot")]
	public IActionResult Reboot()
	{
		_ = Process.Start("sudo", "reboot");
		return Ok("rebooting ...");
	}

	[HttpPost("halt")]
	public IActionResult Halt()
	{
		_ = Process.Start("sudo", "halt");
		return Ok("halting ...");
	}

	[HttpGet("meminfo")]
	public IActionResult Meminfo()
	{
		var p = Process.Start("cat", "/proc/meminfo");
		using Process process = new();
        process.StartInfo.FileName = "cat";
		process.StartInfo.Arguments = "/proc/meminfo";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        // Synchronously read the standard output of the spawned process.
        StreamReader reader = process.StandardOutput;
        string output = reader.ReadToEnd();
        process.WaitForExit();
		return Ok(output);
	}
}