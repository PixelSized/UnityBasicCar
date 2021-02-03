using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//This script is used to simulate how a basic car would react within unity, it is KINDA realistic, 
//however still needs alot of work, chances are i would not work on this again, atleast not within this project

//if you are using this i would not mind if you gave me a bit of credit, unless of course you have bought this off me :)

//Made on 31/01/2021
//Last modified 03/02/2021

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour {

	public float idealRPM = 500f; //Rate of which we receieve torque
	public float maxRPM = 1000f; //When we reach this rpm, the engine will not be adding more torque to the wheels

	public Transform centerOfGravity; //The center of gravity controlled where the transform of this object is

	public WheelCollider wheelFR; //Front right wheel collider
	public WheelCollider wheelFL; //Front left wheel collider
	public WheelCollider wheelRR; //Rear right wheel collider
	public WheelCollider wheelRL; //Rear right wheel collider

	[Range(2,10)] //Thought adding a slider would be a cool addition ;)
	public float turnRadius = 6f; //The turning radius
	public float torque = 25f; //The amount of torque the "engine" would add to the wheels
	public float brakeTorque = 100f; //The amount of brake torque the "brake pads" will add to the wheels

	[Range(0,40000)]
	public float AntiRoll = 20000.0f; //The amount of anti-roll the vehicle has

	public enum DriveMode { Front, Rear, All }; //The drive mode the vehicle has
	public DriveMode driveMode = DriveMode.Rear; //Setting the drive mode default as rear

	public Text speedText; //The text for the speedometer

	private Rigidbody rb; //The rigidbody which will be automatically applied when this script is dragged onto the object

	void Start() {
		rb = GetComponent<Rigidbody>(); //Just getting the rigid body
		rb.centerOfMass = centerOfGravity.localPosition; //Setting the center of gravity for the car (heaviest point of car)
	}

	public float Speed() {
		return wheelRR.radius * Mathf.PI * wheelRR.rpm * 60f / 1000f; //Calculating the speed of the car
	}

	public float Rpm() {
		return wheelRL.rpm; //Returning how many revolutions of the wheels of the car
	}

	void FixedUpdate () {

		if(speedText!=null)
			speedText.text = "Speed: " + Speed().ToString("f0") + " km/h"; //Formatting the speed to display on screen

		float scaledTorque = Input.GetAxis("Vertical") * torque; //The scaled torque of the car

		if(wheelRL.rpm < idealRPM)
			scaledTorque = Mathf.Lerp(scaledTorque/10f, scaledTorque, wheelRL.rpm / idealRPM );
		else 
			scaledTorque = Mathf.Lerp(scaledTorque, 0,  (wheelRL.rpm-idealRPM) / (maxRPM-idealRPM) );

		DoRollBar(wheelFR, wheelFL); //Simulating anti-roll bars
		DoRollBar(wheelRR, wheelRL); //Simulating anti-roll bars

		wheelFR.steerAngle = Input.GetAxis("Horizontal") * turnRadius; //Rotation of the wheels
		wheelFL.steerAngle = Input.GetAxis("Horizontal") * turnRadius; //Rotation of the wheels

		wheelFR.motorTorque = driveMode==DriveMode.Rear  ? 0 : scaledTorque; //If this tyre is apart of the drivemode, apply torque when given acceleration input
		wheelFL.motorTorque = driveMode==DriveMode.Rear  ? 0 : scaledTorque; //If this tyre is apart of the drivemode, apply torque when given acceleration input
		wheelRR.motorTorque = driveMode==DriveMode.Front ? 0 : scaledTorque; //If this tyre is apart of the drivemode, apply torque when given acceleration input
		wheelRL.motorTorque = driveMode==DriveMode.Front ? 0 : scaledTorque; //If this tyre is apart of the drivemode, apply torque when given acceleration input

		if(Input.GetButton("Fire1")) {
			wheelFR.brakeTorque = brakeTorque; //Easy way to simulate braking force on all wheels based on if there is brake input
			wheelFL.brakeTorque = brakeTorque;
			wheelRR.brakeTorque = brakeTorque; 
			wheelRL.brakeTorque = brakeTorque; 
		}
		else {
			wheelFR.brakeTorque = 0; //and then obviously if there no brake input, then apply no brake force
			wheelFL.brakeTorque = 0;
			wheelRR.brakeTorque = 0;
			wheelRL.brakeTorque = 0;
		}
	}

	//Method for simulating anti-roll bars, this will stop the car from merking itself everytime you turn,
	//you could also add downforce but i felt anti-roll would suit better and be more realistic 
	void DoRollBar(WheelCollider WheelL, WheelCollider WheelR) {
		WheelHit hit;
		float travelL = 1.0f;
		float travelR = 1.0f;
		
		bool groundedL = WheelL.GetGroundHit(out hit);
		if (groundedL)
			travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;
		
		bool groundedR = WheelR.GetGroundHit(out hit);
		if (groundedR)
			travelR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;
		
		float antiRollForce = (travelL - travelR) * AntiRoll;
		
		if (groundedL)
			rb.AddForceAtPosition(WheelL.transform.up * -antiRollForce,
			                             WheelL.transform.position); 
		if (groundedR)
			rb.AddForceAtPosition(WheelR.transform.up * antiRollForce,
			                             WheelR.transform.position); 
		//lots of math.
	}

}
