using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] PlayerSettings setup;
	public Rigidbody myRigidbody;
	public Camera mainCamera;

	private Vector3 euler = new Vector3();
	private Quaternion cameraRotation;
	private Vector3 cameraPosition;
	private long lastPressedSpace=0;
	private long lastPressedW=0;
	public State state = State.Creative_Walking;
	private bool running=false;
	private float wobble = 0;
	private float wobbleIntensity = 0;

	public enum State
	{
		Survival,
		Creative_Walking,
		Creative_Flying,
		Spectator
	}

	[System.Serializable]
	public class PlayerSettings
	{
		public float walkSpeed = 1; 
		public float walkForce = 10000; 
		public float fallSpeed = 500; 
		public float fallForce = 1000;
		public float runSpeed = 3; 
		public float runForce = 20000;
		public float fieldOfView = 60;
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		cameraRotation = Quaternion.Euler(euler);
		cameraPosition = transform.position + new Vector3(0, 0.5f, 0);
	}

	void Update()
	{
		myRigidbody.isKinematic = state == State.Spectator;

		HandleCursorLock();		

		Vector2 movementInput = GetMovementInput();
		UpdateRunningState(movementInput);
		UpdateWobbleEffect(movementInput);

		if (state < State.Spectator)
		{
			Movement(movementInput);
		}
		else
		{
			SpectatorMovement(movementInput);
		}
		
		CameraUpdate();
	}

	private void HandleCursorLock()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = (Cursor.lockState == CursorLockMode.None);
		}
	}

	private Vector2 GetMovementInput()
	{
		Vector2 movement = new Vector2();
		movement.x += Input.GetKey(KeyCode.D) ? 1 : 0;
		movement.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
		movement.y += Input.GetKey(KeyCode.W) ? 1 : 0;
		movement.y -= Input.GetKey(KeyCode.S) ? 1 : 0;
		return movement;
	}

	private void UpdateRunningState(Vector2 movementInput)
	{
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			if (movementInput != Vector2.zero) running = true;
		}
		if (Input.GetKeyDown(KeyCode.W))
		{
			long timestamp = TimeStamp();
			if (timestamp < lastPressedW + 500)
			{
				running = true;
				lastPressedW = 0;
			}
			else
			{
				lastPressedW = TimeStamp();
			}
		}
		if (movementInput.y <= 0) running = false;
	}

	private void UpdateWobbleEffect(Vector2 movementInput)
	{
		float wobbleTargetIntensity = movementInput != Vector2.zero ? (running ? 2f : 1f) : 0;
		if (state > (State)1) wobbleTargetIntensity = 0;
		wobbleIntensity = Mathf.Lerp(wobbleIntensity, wobbleTargetIntensity, Time.deltaTime * 16f);
	}

	private long TimeStamp()
	{
		return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}

	private void CameraUpdate()
	{
		euler.x -= Input.GetAxis("Mouse Y") * 2f;
		euler.y += Input.GetAxis("Mouse X") * 2f;
		euler.x = Mathf.Clamp(euler.x, -89.99f, 89.99f);
		cameraRotation = Quaternion.Euler(euler);
		mainCamera.transform.rotation = cameraRotation;

		Vector3 camTargetPosition = transform.position + new Vector3(0, 0.5f, 0);
		cameraPosition = Vector3.Lerp(cameraPosition, camTargetPosition, Time.deltaTime * 20f);
		mainCamera.transform.position = cameraPosition;

		CameraWobble();
		CameraFOV();
	}

	private void CameraWobble(){
		wobble += Time.deltaTime * 8f * (running ? 1.3f : 1f);
		mainCamera.transform.Rotate(Vector3.forward, Mathf.Sin(wobble) * 0.2f * wobbleIntensity);
		mainCamera.transform.Rotate(Vector3.right, Mathf.Sin(wobble * 2f) * 0.3f * wobbleIntensity);
		mainCamera.transform.Rotate(Vector3.up, -Mathf.Sin(wobble ) * 0.2f * wobbleIntensity);
		mainCamera.transform.position += (mainCamera.transform.up * (Mathf.Sin(wobble*2f) * 0.05f * wobbleIntensity));
		mainCamera.transform.position += (mainCamera.transform.right * (Mathf.Sin(wobble) * 0.05f * wobbleIntensity));
	}

	private void CameraFOV(){
		float fov = setup.fieldOfView + (running ? 10 : 0);
		//if (movement == Vector2.zero) fov = Input.GetKey(KeyCode.Tab) ? 10 : fov;
		mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, fov, Time.deltaTime * 8f);
		if (Input.GetKey(KeyCode.Tab)) mainCamera.fieldOfView = 20;
		if (Input.GetKeyUp(KeyCode.Tab)) mainCamera.fieldOfView = fov;
	}

	private void Movement(Vector2 movementInput)
	{	
		myRigidbody.linearVelocity = Vector3.Lerp(myRigidbody.linearVelocity, GetVelocityY(), Time.deltaTime * 8f);

		float moveForce = running ? setup.runForce : setup.walkForce;
		myRigidbody.AddForce(GetCameraForward() * (movementInput.y * (moveForce * Time.deltaTime)));
		myRigidbody.AddForce(GetCameraRight() * (movementInput.x * (moveForce * Time.deltaTime)));
		if (state < (State)2)
		{
			myRigidbody.AddForce(Vector3.down * (setup.fallForce * Time.deltaTime));
		}

		float moveSpeed = running ? setup.runSpeed : setup.walkSpeed;
		Vector3 velocityWalk = Vector3.ClampMagnitude(GetVelocityXZ(), moveSpeed);
		Vector3 velocityFall = Vector3.ClampMagnitude(GetVelocityY(), setup.fallSpeed);
		Vector3 targetVelocity = velocityWalk + velocityFall;

		WalkingFlyingToggle();
		if (state == State.Creative_Flying)
		{
			targetVelocity.y = GetVerticalMovementInput();
		}

		//myRigidbody.velocity = Vector3.Lerp(myRigidbody.velocity, targetVelocity, Time.deltaTime * 8f);
		myRigidbody.linearVelocity = targetVelocity;
	}

	private void WalkingFlyingToggle()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			long timestamp = TimeStamp();
			if (timestamp < lastPressedSpace + 500)
			{
				if (state == State.Creative_Walking)
				{
					state = State.Creative_Flying;
					myRigidbody.useGravity = false;
				}
				else if (state == State.Creative_Flying)
				{
					state = State.Creative_Walking;
					myRigidbody.useGravity = true;
				}
				lastPressedSpace = 0;
			}
			else
			{
				lastPressedSpace = TimeStamp();
			}
		}
	}

	private void SpectatorMovement(Vector2 movementInput)
	{
		ApplySpectatorPlanarMovement(movementInput);
		ApplySpectatorAltitudeAdjustment();
	}

	private void ApplySpectatorPlanarMovement(Vector2 movementInput)
	{
		float moveSpeed = running ? 16 : 8;
		transform.position += GetCameraForward() * (movementInput.y * Time.deltaTime * moveSpeed);
		transform.position += GetCameraRight() * (movementInput.x * Time.deltaTime * moveSpeed);
	}

	private void ApplySpectatorAltitudeAdjustment()
	{
		transform.position += Vector3.up * (GetVerticalMovementInput() * Time.deltaTime);
	}

	private float GetVerticalMovementInput()
	{
		float altitudeChange = 0;
		if (Input.GetKey(KeyCode.Space))
		{
			altitudeChange += 8;
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			altitudeChange -= 8;
		}
		return altitudeChange;
	}

	private Vector3 GetCameraForward()
	{
		Vector3 forward = mainCamera.transform.forward;
		forward.y = 0;
		forward.Normalize();
		return forward;
	}

	private Vector3 GetCameraRight()
	{
		Vector3 right = mainCamera.transform.right;
		right.y = 0;
		right.Normalize();
		return right;
	}

	private Vector3 GetVelocityY(){
		Vector3 velocity = myRigidbody.linearVelocity;
		velocity.x = 0;
		velocity.z = 0;
		return velocity;
	}

	private Vector3 GetVelocityXZ(){
		Vector3 velocity = myRigidbody.linearVelocity;
		velocity.y = 0;
		return velocity;
	}
}
