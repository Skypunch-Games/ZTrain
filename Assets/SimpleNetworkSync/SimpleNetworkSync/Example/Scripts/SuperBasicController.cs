//Copyright 2019, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Networking;

public class SuperBasicController : GenericNetworkBehaviour
{

	[Range(0, 100f)]
	public float turnSpeed = 60f;
	[Range(0, 4f)]
	public float moveSpeed = 4f;

	/// Store transform data from the last fixedUpdate
	private Vector3 targRotDelta, targPosDelta;

	private Animator animator;
	private SimpleSyncAnimator syncAnimator;
	private SimpleSyncTransform syncTransform;
	private Rigidbody rb;
	private Rigidbody2D rb2d;

	private bool triggerJump;
	private bool triggerFade;
	private bool triggerTurnLeft;
	private bool triggerUpperBodyRun;
	private bool triggerUpperBodyIdle;
	private bool triggerTeleport;
	private bool freakingOut;

	// Start is called before the first frame update
	protected override void Awake()
	{
		base.Awake();
		animator = GetComponent<Animator>();
		syncAnimator = GetComponent<SimpleSyncAnimator>();
		syncTransform = GetComponent<SimpleSyncTransform>();
		rb = GetComponent<Rigidbody>();
		rb2d = GetComponent<Rigidbody2D>();

	}

	private float appliedDeltaT;
	private void Update()
	{
		if (!IsMine)
			return;

		float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
		Interpolate(t);

		if (Input.GetKeyDown(KeyCode.Space))
			triggerJump = true;

		if (Input.GetKeyDown(KeyCode.Alpha2))
			triggerFade = true;

		if (Input.GetKeyDown(KeyCode.Alpha1))
			triggerTurnLeft = true;

		if (Input.GetKeyDown(KeyCode.R))
		{
			if (freakingOut)
				triggerUpperBodyIdle = true;
			else
				triggerUpperBodyRun = true;

			freakingOut = !freakingOut;
		}

		if (Input.GetKeyDown(KeyCode.T))
			triggerTeleport = true;
	}

	void FixedUpdate()
	{
		if (!IsMine)
			return;

		Vector3 move = new Vector3(0, 0, 0);
		Vector3 turn = new Vector3(0, 0, 0);

		if (animator)
		{
			if (Input.GetKey(KeyCode.W))
			{
				animator.SetBool("walking", true);
				animator.SetFloat("speed", 1);
			}
			else if (Input.GetKey(KeyCode.S))
			{
				animator.SetBool("walking", true);
				animator.SetFloat("speed", -.5f);
			}

			else
			{
				animator.SetBool("walking", false);
				animator.SetFloat("speed", 0);
			}

			if (triggerJump)
			{
				if (syncAnimator)
					syncAnimator.SetTrigger("jump");

				triggerJump = false;
			}

			else if (triggerTurnLeft)
			{
				if (syncAnimator)
					syncAnimator.SetTrigger("turnLeft");

				triggerTurnLeft = false;
			}

			if (triggerFade)
			{
				if (syncAnimator)
					syncAnimator.CrossFadeInFixedTime("Jump", .25f);

				triggerFade = false;
			}

			if (triggerUpperBodyRun)
			{
				if (syncAnimator)
					syncAnimator.SetTrigger("upperBodyRun");

				triggerUpperBodyRun = false;
			}
			else if (triggerUpperBodyIdle)
			{
				if (syncAnimator)
					syncAnimator.SetTrigger("upperBodyIdle");

				triggerUpperBodyIdle = false;
			}
		}

		if (!animator || !animator.applyRootMotion)
		{
			if (Input.GetKey(KeyCode.W))
			{
				move += Vector3.forward;
			}
			else if (Input.GetKey(KeyCode.S))
			{
				move -= Vector3.forward;
			}
		}

		if (Input.GetKey(KeyCode.A))
			move -= Vector3.right;

		if (Input.GetKey(KeyCode.D))
			move += Vector3.right;

		if (Input.GetKey(KeyCode.E))
			turn += Vector3.up;

		if (Input.GetKey(KeyCode.Q))
			turn -= Vector3.up;

		if (Input.touchCount > 0)
		{
			var touch = Input.GetTouch(0);
			Vector2 normTouch = new Vector2(touch.rawPosition.x / Screen.width, touch.rawPosition.y / Screen.height);

			if (normTouch.y > .66f)
			{
				//if (normTouch.x > .66f)
				//	triggerFire = true;
				//else 
				if (normTouch.x < .33f)
					triggerJump = true;
			}
			else if (normTouch.y < .33f)
			{
				if (normTouch.x > .66f)
					move += Vector3.right;
				else if (normTouch.x < .33f)
					move -= Vector3.right;
				else
				{
					if (animator)
					{
						animator.SetBool("walking", true);
						animator.SetFloat("speed", -0.5f);
					}
				}
			}
			else
			{
				if (normTouch.x > .66f)
					turn += Vector3.up;
				else if (normTouch.x < .33f)
					turn -= Vector3.up;
				else
				{
					if (animator)
					{
						animator.SetBool("walking", true);
						animator.SetFloat("speed", 1f);
					}
				}
			}

			Debug.Log(normTouch);

		}

		Interpolate(1);

		Move(move, turn);

		appliedDeltaT = 0;


		if (triggerTeleport)
		{
			//triggerTeleport = true;
			transform.position = new Vector3(0, 0, 0);

			if (syncTransform)
				syncTransform.HasTeleported = true;

			triggerTeleport = false;
		}
	}

	private void OnAnimatorMove()
	{
		if (!IsMine)
			return;

		animator.ApplyBuiltinRootMotion();

		if (rb)
		{
			if (rb.isKinematic == false)
			{
				transform.position = animator.rootPosition;
				transform.rotation = animator.rootRotation;
			}
		}
		else if (rb2d)
		{
			if (rb2d.isKinematic == false)
			{
				transform.position = animator.rootPosition;
				transform.rotation = animator.rootRotation;
			}
		}
	}

	private void Move(Vector3 move, Vector3 turn)
	{
		targRotDelta = turn * turnSpeed * Time.fixedDeltaTime;
		targPosDelta = move * moveSpeed * Time.fixedDeltaTime;
	}

	void Interpolate(float t)
	{
		t -= appliedDeltaT;

		appliedDeltaT += t;

		transform.rotation = (transform.rotation * Quaternion.Euler(targRotDelta * t));
		transform.position += transform.rotation * (targPosDelta * t);
	}

}

