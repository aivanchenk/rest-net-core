namespace Servers;

using Microsoft.AspNetCore.Mvc;

[Route("/farm")]
[ApiController]
public class FarmController : ControllerBase
{

	private readonly FarmLogic mLogic;


    public FarmController(FarmLogic logic)
    {
        this.mLogic = logic;
    }


    [HttpPost("/submitFood")]
    public ActionResult<SubmissionResult> SubmitFood(double amount)
    {
        return mLogic.SubmitFood(amount);
    }	
    
    [HttpPost("/submitWater")]
    public ActionResult<SubmissionResult> SubmitWater(double amount)
    {
        return mLogic.SubmitWater(amount);
    }
}