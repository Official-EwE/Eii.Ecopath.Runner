using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using Eii.Ecopath.Runner.Datamodel.Utilities;
using Eii.Ecopath.Runner.Services.Runtime;
using Microsoft.AspNetCore.Mvc;

namespace EwERunApi.Controllers
{
    [ApiController]
    [Route("eweRun")]
    public class EwERunController : ControllerBase
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Execute an EwE run from a posted <see cref="cEwERunInstructions"/>
        /// configuration object.
        /// </summary>
        /// <param name="instructions">The full run configuration.</param>
        /// <param name="inputFolder">Path to the folder where input files are located.</param>
        /// <param name="outputFolder">Path to the folder where output files are written.</param>
        /// <returns>200 OK on success, 500 Problem on failure.</returns>
        // --------------------------------------------------------------------
        [HttpPost(Name = "PostEwERun")]
        public IActionResult Post([FromBody] cEwERunInstructions instructions, [FromQuery] string? inputFolder = null, [FromQuery] string? outputFolder = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(inputFolder, nameof(inputFolder));
            ArgumentException.ThrowIfNullOrEmpty(outputFolder, nameof(outputFolder));

            string baseDir = AppContext.BaseDirectory;

            // You only have to pass the RunConfigFile so it can determine the directory of that file. This used to be the "AnchovyBay_runinfo.json", but in the API that is replaced by the posted instructions object. 
            // This should be changed in the EwERunConsole project. You should be able to pass the input folder.
            instructions.RunConfigFile = Path.Combine(baseDir, inputFolder, instructions.Configuration.ModelFile);
            instructions.OutputFolder = Path.Combine(baseDir, outputFolder);

            Directory.CreateDirectory(instructions.OutputFolder);

            using (var cc = new cConsoleCopy(Path.Combine(instructions.OutputFolder, "EwERunConsole_log.txt")))
            {
                cEwEEngine engine = new cEwEEngine(instructions);
                if (engine.Run())
                    return Ok("Run completed");

                return Problem("Run errors encountered");
            }
        }
    }
}
