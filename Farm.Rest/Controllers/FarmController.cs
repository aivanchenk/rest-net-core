namespace Servers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller responsible for handling farm-related operations,
/// including submission of food and water resources to the farm logic layer.
/// </summary>

[Route("/farm")]
[ApiController]
public class FarmController : ControllerBase
{

    private readonly FarmLogic mLogic;

    /// <summary>
    /// Initializes a new instance of the <see cref="FarmController"/> class.
    /// </summary>
    /// <param name="logic">An instance of the farm logic handler that manages core farm operations.</param>
    public FarmController(FarmLogic logic)
    {
        this.mLogic = logic;
    }


    /// <summary>
    /// Submits a specified amount of food to the farm for processing.
    /// </summary>
    /// <param name="amount">The amount of food to be added to the farm.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="SubmissionResult"/>
    /// with the status and outcome of the food submission.
    /// </returns>
    [HttpPost("/submitFood")]
    public ActionResult<SubmissionResult> SubmitFood(double amount)
    {
        return mLogic.SubmitFood(amount);
    }

    /// <summary>
    /// Submits a specified amount of water to the farm for processing.
    /// </summary>
    /// <param name="amount">The amount of water to be added to the farm.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="SubmissionResult"/>
    /// with the status and outcome of the water submission.
    /// </returns>
    [HttpPost("/submitWater")]
    public ActionResult<SubmissionResult> SubmitWater(double amount)
    {
        return mLogic.SubmitWater(amount);
    }
}