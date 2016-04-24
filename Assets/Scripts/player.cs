using System;
using UnityEngine;

public class player : MonoBehaviour
{
	public static float sensitivity = 3f;

	public static float controlSensitivity = 2f;

	private float speed = 2f;

	private float airSpeed = 0.9f;

	private float glide = 1000f;

	private float jumpCd;

	private float airForce = 0.1f;

	private Rigidbody rig;

	public Transform camHolder;

	private bool canMove = true;

	private bool dead;

	private bool frozen;

	public AudioClip jump;

	public AudioClip land;

	public AudioSource myAudioSource;


	[HideInInspector]
	public bool hasTouchedGround = true;

	private float climbCD;

	public LayerMask mask;

	private float framesSinceStart;

	private bool running;

	private bool walking;

	[HideInInspector]
	public Vector3 playerMadeVel = Vector3.zero;

	private Vector3 playerTargetVector = Vector3.zero;

	private float stepCD;

	public AudioSource stepAU;

	public AudioClip[] step;

	private Vector3 rigMove = Vector3.zero;

	private Vector3 rigMoveAir = Vector3.zero;

	public float stepSpeed = 0.2f;

	private bool canForward;

	[HideInInspector]
	public float _xValue;

	[HideInInspector]
	public float _yValue;

	[HideInInspector]
	public float lastGrounded;

	private void Start()
	{
		this.rig = base.gameObject.GetComponent<Rigidbody>();
		Debug.Log("started");
	}

	public bool getWalkingstate()
	{
		return this.walking;
	}

	public bool getRunningState()
	{
		return this.running;
	}

	private void CheckInput()
	{
		this.playerTargetVector = Vector3.zero;
		if ((Input.GetButton("Left") ) && (!Input.GetButton("Right")  ))
		{
			this.playerTargetVector = -base.transform.right;
			if (Input.GetButton("Forward")  )
			{
				this.playerTargetVector = Vector3.Normalize(-base.transform.right + base.transform.forward * 1.2f);

			}
		}
		if ((Input.GetButton("Right") ) && (!Input.GetButton("Left")  ))
		{
			this.playerTargetVector = base.transform.right;
			if (Input.GetButton("Forward")  )
			{
				this.playerTargetVector = Vector3.Normalize(base.transform.right + base.transform.forward * 1.2f);
			}
		}
		if (Input.GetButton("Forward") )
		{
			this.playerTargetVector = base.transform.forward;
			Debug.Log("forward");
		}
		else if (Input.GetButton("Back")  )
		{
			this.playerTargetVector = -base.transform.forward;
			Debug.Log("back");
		}
	}

	private void JumpRayHit()
	{
		if (Input.GetButton("Jump") )
		{
			this.rig.AddForce(base.transform.forward * Time.deltaTime * 30f, ForceMode.VelocityChange);
			this.rig.AddForce(base.transform.up * Time.deltaTime * 90f, ForceMode.VelocityChange);
			Debug.Log("jump");
//			Camera.main.GetComponent<cameraEffects>().SetShake(0.15f, Vector3.zero);
		}
		this.canForward = false;
	}

	private void Update()
	{
		this.canForward = true;
		this.CheckInput();
		this.framesSinceStart += 1f;
		if (this.framesSinceStart > 10f)
		{
			RaycastHit raycastHit;
			Physics.Raycast(base.transform.position + Vector3.down / 2f, base.transform.forward, out raycastHit, 1f, this.mask);
			RaycastHit raycastHit2;
			Physics.Raycast(base.transform.position + Vector3.up / 2f, base.transform.forward, out raycastHit2, 1f, this.mask);
			RaycastHit raycastHit3;
			Physics.Raycast(base.transform.position, base.transform.forward, out raycastHit3, 1f, this.mask);
			if (raycastHit.transform != null)
			{
				if (!raycastHit.collider.isTrigger && !this.hasTouchedGround && raycastHit.transform.tag == "ground")
				{
					this.JumpRayHit();
				}
			}
			else if (raycastHit2.transform != null)
			{
				if (!raycastHit2.collider.isTrigger && !this.hasTouchedGround && raycastHit2.transform.tag == "ground")
				{
					this.JumpRayHit();
				}
			}
			else if (raycastHit3.transform != null && !raycastHit3.collider.isTrigger && !this.hasTouchedGround && raycastHit3.transform.tag == "ground")
			{
				this.JumpRayHit();
			}
//			if (base.transform.position.y < -500f && this.man != null)
//			{
//				this.gMan.LoseLevel();
//			}
//			if (Input.GetKeyDown(KeyCode.R) && !this.man)
//			{
//				car.numberOfTrucks = 0;
//				Application.LoadLevel(Application.loadedLevel);
//			}
			this.walking = false;
			this.running = false;
			if (this.canMove)
			{
				if (this.lastGrounded < 0.2f && this.jumpCd > 0.3f)
				{
					this.rig.AddForce(Vector3.down * Time.deltaTime * 500f);
				}
				this._xValue = Input.GetAxis("Mouse X");
				this._yValue = -Input.GetAxis("Mouse Y");
//				if (Mathf.Abs(InputManager.ActiveDevice.RightStickX.Value) > 0.01f)
//				{
//					this._xValue = InputManager.ActiveDevice.RightStickX.Value * player.controlSensitivity;
//				}
//				if (Mathf.Abs(InputManager.ActiveDevice.RightStickY.Value) > 0.01f)
//				{
//					this._yValue = -InputManager.ActiveDevice.RightStickY.Value * player.controlSensitivity;
//				}
				base.transform.Rotate(Vector3.up * player.sensitivity * this._xValue);
				this.camHolder.transform.Rotate(Vector3.right * player.sensitivity * this._yValue);
				if ((Input.GetButton("Forward")  ) && this.canForward)
				{
					this.walking = true;
					this.rig.AddForce(base.transform.forward * Time.deltaTime * this.glide * 0.7f);
					this.playerMadeVel += base.transform.forward * Time.deltaTime * this.glide * 0.7f;
					if ((Input.GetButton("Sprint") ) && this.lastGrounded < this.airForce)
					{
						this.running = true;
						this.rigMove = base.transform.forward * Time.deltaTime * this.speed * 1f + this.rigMove;
						if (this.hasTouchedGround)
						{
							this.rig.AddForce(base.transform.forward * Time.deltaTime * this.glide * 1f);
							this.playerMadeVel += base.transform.forward * Time.deltaTime * this.glide * 1f;
						}
					}
					if (this.hasTouchedGround)
					{
						this.rigMove = base.transform.forward * Time.deltaTime * this.speed + this.rigMove;
					}
					else
					{
						this.rigMove += base.transform.forward * Time.deltaTime * this.airSpeed * 2f;
						if (Time.timeScale < 0.75f)
						{
							this.rig.AddForce(base.transform.forward * Time.deltaTime * this.glide / 2f);
							this.playerMadeVel += base.transform.forward * Time.deltaTime * this.glide / 2f;
						}
					}
				}
				if (Input.GetButton("Back") )
				{
					this.walking = true;
					this.rigMove = base.transform.forward * Time.deltaTime * -this.speed / 2f + this.rigMove;
					if (this.hasTouchedGround)
					{
						this.rig.AddForce(base.transform.forward * Time.deltaTime * -this.glide);
						this.playerMadeVel += base.transform.forward * Time.deltaTime * -this.glide;
					}
					else
					{
						this.rigMove += base.transform.forward * Time.deltaTime * -this.airSpeed;
					}
				}
				if (Input.GetButton("Left")  )
				{
					this.walking = true;
					this.rigMove = base.transform.right * Time.deltaTime * -this.speed / 4f + this.rigMove;
					if (this.hasTouchedGround)
					{
						this.rig.AddForce(base.transform.right * Time.deltaTime * -this.glide);
						this.playerMadeVel += base.transform.right * Time.deltaTime * -this.glide;
					}
					else
					{
						this.rigMove += base.transform.right * Time.deltaTime * -this.airSpeed * 0.5f;
						this.rig.AddForce(base.transform.right * Time.deltaTime * -this.glide / 2f);
						this.playerMadeVel += base.transform.right * Time.deltaTime * -this.glide / 2f;
					}
				}
				if (Input.GetButton("Right")  )
				{
					this.walking = true;
					this.rigMove = base.transform.right * Time.deltaTime * this.speed / 4f + this.rigMove;
					if (this.hasTouchedGround)
					{
						this.rig.AddForce(base.transform.right * Time.deltaTime * this.glide);
						this.playerMadeVel += base.transform.right * Time.deltaTime * this.glide;
					}
					else
					{
						this.rigMove += base.transform.right * Time.deltaTime * this.airSpeed * 0.5f;
						this.rig.AddForce(base.transform.right * Time.deltaTime * this.glide / 2f);
						this.playerMadeVel += base.transform.right * Time.deltaTime * this.glide / 2f;
					}
				}
				if ((Input.GetButton("Jump") ) && this.lastGrounded < 0.5f && this.jumpCd > 0.1f && this.hasTouchedGround)
				{
					this.Jump(11f);
					this.rig.AddForce(this.playerTargetVector * 1f, ForceMode.VelocityChange);
					this.jumpCd = 0f;
				}
				if (Input.GetButton("Jump") )
				{
					this.rig.AddForce(base.transform.up * 500f * Time.deltaTime);
				}
				this.lastGrounded += Time.deltaTime;
				if (this.hasTouchedGround)
				{
					this.jumpCd += Time.deltaTime;
				}
			}
			this.rig.MovePosition(base.transform.position + this.rigMove);
			if (this.lastGrounded > 0.2f)
			{
//				this.camAnim.SetInteger("state", 0);
			}
			else if (this.running)
			{
//				this.camAnim.SetInteger("state", 2);
				this.PlayStep(this.stepSpeed * 0.5f);
			}
			else if (this.walking)
			{
//				this.camAnim.SetInteger("state", 1);
				this.PlayStep(this.stepSpeed);
			}
			else
			{
//				this.camAnim.SetInteger("state", 0);
			}
		}
	}

	public void Jump(float force)
	{
		this.myAudioSource.PlayOneShot(this.jump);
		this.hasTouchedGround = false;
//		this.camForce.AddRot();
		this.rig.velocity = new Vector3(this.rig.velocity.x, 0f, this.rig.velocity.z);
		this.rig.AddForce(base.transform.up * force, ForceMode.VelocityChange);
	}

	private void PlayStep(float f)
	{
		if (this.stepCD > f && this.lastGrounded < 0.1f)
		{
			this.stepAU.pitch = UnityEngine.Random.Range(0.9f, 1.05f);
			this.stepAU.PlayOneShot(this.step[UnityEngine.Random.Range(0, this.step.Length)]);
			this.stepCD = 0f;
		}
		if (this.hasTouchedGround)
		{
			this.stepCD += Time.deltaTime;
		}
	}

	private void FixedUpdate()
	{
		if (Mathf.Abs(this.rig.velocity.y) > 35f)
		{
			this.rig.velocity = new Vector3(this.rig.velocity.x, this.rig.velocity.y * 0.996f, this.rig.velocity.z);
		}
		if (this.hasTouchedGround)
		{
			this.rigMove *= 0.7f;
		}
		else
		{
			this.rigMove *= 0.8f;
			if (this.walking)
			{
				this.rig.velocity *= 0.9995f;
			}
		}
		this.playerMadeVel *= 0.995f;
		if (this.canMove)
		{
			this.rig.AddForce(Vector3.down * 16f * Time.timeScale);
		}
		if (this.frozen)
		{
			this.rig.velocity *= 0.93f;
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (this.lastGrounded > 0.2f)
		{
//			this.camForce.AddForce(Vector3.down * 5f);
//			this.camForce.AddRot();
			this.myAudioSource.PlayOneShot(this.land);
			this.rigMove *= 0f;
			Debug.Log("HELLO");
		}
		this.hasTouchedGround = true;
	}

	private void OnCollisionStay(Collision other)
	{
//		if (other.transform.tag == "kill" && this.man != null)
//		{
////			this.gMan.LoseLevel();
//		}
		this.lastGrounded = 0f;
	}

	private void StopMoving()
	{
//		this.canMove = false;
//		this.Freeze();
	}

	public void Freeze()
	{
//		this.gMan.LoseLevel();
//		this.rig.useGravity = false;
//		this.frozen = true;
		Debug.Log("freeze");
	}
}
