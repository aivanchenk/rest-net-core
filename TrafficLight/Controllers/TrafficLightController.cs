namespace Servers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Service. Class must be marked public, otherwise ASP.NET core runtime will not find it.
/// 
/// Look into FromXXX attributes if you need to map inputs to custom parts of HTTP request.
/// </summary>
[Route("/trafficLight")] [ApiController]
public class TrafficLightController : ControllerBase
{
	/// <summary>
	/// Service logic. This is created in Server.StartServer() and received through DI in constructor.
	/// </summary>
	private readonly TrafficLightLogic mLogic;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="logic">Logic to use. This will get passed through DI.</param>
	public TrafficLightController(TrafficLightLogic logic)
	{
		this.mLogic = logic;
	}

	/// <summary>
	/// Get next unique ID from the server. Is used by cars to acquire client ID's.
	/// </summary>
	/// <returns>Unique ID.</returns>
	[HttpGet("/getUniqueId")]
	public ActionResult<int> GetUniqueId() 
	{
		return mLogic.GetUniqueId();
	}

	/// <summary>
	/// Get current light state.
	/// </summary>
	/// <returns>Current light state.</returns>				
	[HttpGet("/getLightState")]
	public ActionResult<LightState> GetLightState()
	{
		return mLogic.GetLightState();
	}

	/// <summary>
	/// Queue give car at the light. Will only succeed if light is red.
	/// </summary>
	/// <param name="car">Car to queue.</param>
	/// <returns>True on success, false on failure.</returns>
	[HttpPost("/queue")]
	public ActionResult<bool> Queue(CarDesc car)
	{
		return mLogic.Queue(car);
	}
	
	/// <summary>
	/// Tell if car is first in line in queue.
	/// </summary>
	/// <param name="carId">ID of the car to check for.</param>
	/// <returns>True if car is first in line. False if not first in line or not in queue.</returns>
	[HttpGet("/isFirstInLine")]
	public ActionResult<bool> IsFirstInLine(int carId)
	{
		return mLogic.IsFirstInLine(carId);
	}

	/// <summary>
	/// Try passing the traffic light. If car is in queue, it will be removed from it.
	/// </summary>
	/// <param name="car">Car descriptor.</param>
	/// <returns>Pass result descriptor.</returns>
	[HttpPost("/pass")]
	public ActionResult<PassAttemptResult> Pass(CarDesc car)
	{
		return mLogic.Pass(car);
	}	
}