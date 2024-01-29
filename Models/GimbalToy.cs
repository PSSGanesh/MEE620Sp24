//============================================================================
// GimbalToy.cs
//============================================================================
using Godot;
using System;

public partial class GimbalToy : Node3D
{
	Node3D outerRingNode;
	Node3D middleRingNode;
	Node3D innerRingNode;

	Vector3 rot1;    // first rotation vector
	Vector3 rot2;    // second rotation vector
	Vector3 rot3;    // third rotation vector

	Basis basisCalc;
	Vector3 basisX;  // basis vectors
	Vector3 basisY;
	Vector3 basisZ;
	Quaternion ghostQuat;

	GimbalDCM calcDCM;

	enum EulerAngleMode{
		None,
		YPR,
		PYR,
		YRP,
		RPY,
	}

	EulerAngleMode eulerMode;

	//------------------------------------------------------------------------
	// _Ready: Called once when the node enters the scene tree for the first 
	//         time.
	//------------------------------------------------------------------------
	public override void _Ready()
	{
		eulerMode = EulerAngleMode.None;

		outerRingNode = GetNode<Node3D>("OuterRingNode");
		middleRingNode = GetNode<Node3D>("OuterRingNode/MiddleRingNode");
		innerRingNode = GetNode<Node3D>
			("OuterRingNode/MiddleRingNode/InnerRingNode");

		rot1 = new Vector3();
		rot2 = new Vector3();
		rot3 = new Vector3();

		calcDCM = new GimbalDCM();
		basisX = new Vector3();
		basisY = new Vector3();
		basisZ = new Vector3();
		basisCalc = new Basis();
		ghostQuat = new Quaternion();
	}

	//------------------------------------------------------------------------
	// Setup: Takes the requested gimbal configuration and calls the 
	//        appropriate setup routine
	//------------------------------------------------------------------------
	public void Setup(string mm)
	{
		string modeString = mm.ToUpper();

		if(modeString == "YPR")
			SetupYPR();
	}

	//------------------------------------------------------------------------
	// SetAngles:
	//------------------------------------------------------------------------
	public void SetAngles(float angle1, float angle2, float angle3)
	{
		if(eulerMode == EulerAngleMode.YPR)
			SetAnglesYPR(angle1, angle2, angle3);
		else{
			GD.PrintErr("ApplyAngles -- Something's wrong.");
		}

		outerRingNode.Rotation = rot1;
		middleRingNode.Rotation = rot2;
		innerRingNode.Rotation = rot3;

		//process the DCM
		bool dcmChecks = true;
		float det = basisX.X * basisY.Y * basisZ.Z +
			basisX.Z * basisY.X * basisZ.Y +
			basisX.Y * basisY.Z * basisZ.X -

			basisX.Z * basisY.Y * basisZ.X -
			basisX.Y * basisY.X * basisZ.Z -
			basisX.X * basisY.Z * basisZ.Y;

		if(Mathf.Abs(det -1.0f) > 0.002f)
			dcmChecks = false;

		if(Mathf.Abs(basisX.Dot(basisY)) > 0.002f )
			dcmChecks = false;

		if(Mathf.Abs(basisX.Dot(basisZ)) > 0.002f )
			dcmChecks = false;

		if(Mathf.Abs(basisZ.Dot(basisY)) > 0.002f )
			dcmChecks = false;

		if(dcmChecks){
			basisCalc.X = basisX;
			basisCalc.Y = basisY;
			basisCalc.Z = basisZ;

			ghostQuat = basisCalc.GetRotationQuaternion();
		}
		else{
			
		}
	}

	//------------------------------------------------------------------------
	// SetupYPR: Yaw Pich Roll gimbal configuration
	//------------------------------------------------------------------------
	private void SetupYPR()
	{
		eulerMode = EulerAngleMode.YPR;
		
		Node3D outerRing = GetNode<Node3D>("OuterRingNode/OuterRing");
		outerRing.Rotation = new Vector3(0.5f*Mathf.Pi, 0.5f*Mathf.Pi, 0.0f);

		Node3D onn = GetNode<Node3D>("OuterRingNode/OuterNubNode");
		onn.Rotation = new Vector3(0.0f, 0.0f, 0.5f*Mathf.Pi);
	}
	
	//------------------------------------------------------------------------
	// SetAnglesYPR: Yaw Pitch Roll Euler angle application
	//------------------------------------------------------------------------
	private void SetAnglesYPR(float angle1, float angle2, float angle3)
	{
		rot1.Y = angle1;
		rot2.Z = angle2;
		rot3.X = angle3;

		calcDCM.CalcDCM_YPR(angle1, angle2, angle3);
		basisX.X = calcDCM.GetDCM(0,0);
		basisX.Y = calcDCM.GetDCM(1,0);
		basisX.Z = calcDCM.GetDCM(2,0);

		basisY.X = calcDCM.GetDCM(0,1);
		basisY.Y = calcDCM.GetDCM(1,1);
		basisY.Z = calcDCM.GetDCM(2,1);

		basisZ.X = calcDCM.GetDCM(0,2);
		basisZ.Y = calcDCM.GetDCM(1,2);
		basisZ.Z = calcDCM.GetDCM(2,2);
	}

	// public override void _Process(double delta)
	// {
	// }
}
