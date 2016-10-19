using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class RotationController : MonoBehaviour
{
	private enum Option {
		RoomA = 0,
		RoomB = 1
	};

	public GameObject rotatingObject;
	public GUI_Handler gui;
	public float maxDegreesPerSecond = 15f;
	public float maxTrialTime = 30;
	public KeyCode switchRoom = KeyCode.Z;
	public KeyCode selectA = KeyCode.X;
	public KeyCode selectB = KeyCode.C;
	public UnityEngine.UI.Text roomText;

	public float baseRotation = 0f;
	public float rotationProportionMultiplier = 0.6f;
	public float rotationDecrementMultiplier = 0.6f;
	public float rotationDecrement = 0.05f;

	private float maxRotationProportion = 1f;
	private float rotationProportion = 1f;

	public float rotationSpeed {
		get {
			return rotatingRoom == selectedRoom ? baseRotation + maxDegreesPerSecond * rotationProportion * rotationDirection : baseRotation;
		}
	}

	private int rotationDirection;

	private float degreesPerSecond;

	private int failedBlocks = 0;
	private int maxFailedBlocks = 4;
	private bool started = false;

	private int trials = 0;
	private int correctTrials = 0;
	private int minTrialsToPass = 7;
	private int maxTrials = 10;

	private Option rotatingRoom;
	private Option selectedRoom;

	private AudioSource chimes;
	private bool chimesPlayed = false;
	private float timer = 0;

	private System.Random random;

	void Start()
	{
		chimes = this.gameObject.GetComponent<AudioSource>();
		random = new System.Random();
		rotatingRoom = (Option)random.Next(2);
		selectedRoom = Option.RoomA;
		SetRoomText(selectedRoom);
		rotationDirection = 1 - 2 * random.Next(2);
		degreesPerSecond = rotationSpeed;
	}

	void Update()
	{
		if (!started)
			return;
		
		rotatingObject.transform.Rotate(Vector3.up * Time.deltaTime * degreesPerSecond, Space.World);
		timer += Time.deltaTime;
		if (timer > maxTrialTime && !chimesPlayed) {
			chimes.Play();
			chimesPlayed = true;
		}

		if (Input.GetKeyDown(switchRoom) && !gui.isVisible) //Change whether its rotating
		{
			selectedRoom = selectedRoom == Option.RoomA ? Option.RoomB : Option.RoomA;
			degreesPerSecond = rotationSpeed;
			timer = 0;
			chimesPlayed = false;
			SetRoomText(selectedRoom);
		}

		if (Input.GetKeyUp(selectA) || Input.GetKeyUp(selectB) && !gui.isVisible) //User answers
		{
			bool? correct = null;
			if (Input.GetKeyUp(selectA)) //A is rotating
			{
				correctTrials += rotatingRoom == Option.RoomA ? 1 : 0;
				correct = rotatingRoom == Option.RoomA;

				Debug.Log ("A");
			}
			else if (Input.GetKeyUp(selectB)) //B is rotating
			{
				correctTrials += rotatingRoom == Option.RoomB ? 1 : 0;
				correct = rotatingRoom == Option.RoomB;

				Debug.Log ("B");
			}

			Debug.Log(string.Format("Failed Trials: {0}, Number Answered: {1}, Number Right: {2}, Proportion: {3}", failedBlocks, trials, correctTrials, rotationProportion));
			gui.exportData(new List<string> {correct.ToString(), degreesPerSecond.ToString(), correctTrials.ToString(), rotationDecrement.ToString()});

			trials += 1;
			rotationProportion -= rotationDecrement;
			timer = 0;
			chimesPlayed = false;

			if (trials >= maxTrials)
			{
				if (correctTrials >= minTrialsToPass)
				{
					maxRotationProportion *= rotationProportionMultiplier;
					rotationDecrement *= rotationDecrementMultiplier;
				}
				else
				{
					failedBlocks += 1;
				}

				rotationProportion = maxRotationProportion;
				trials = 0;
				correctTrials = 0;
			}

			rotatingRoom = (Option)random.Next(2);
			selectedRoom = Option.RoomA;
			rotationDirection *= -1;
			degreesPerSecond = rotationSpeed;
			SetRoomText(selectedRoom);

			if (failedBlocks >= maxFailedBlocks)
			{
				Application.Quit();
				UnityEditor.EditorApplication.isPlaying = false;
			}
		}
	}

	public void StartExperiment() {
		started = true;
	}

	void SetRoomText(Option o) {
		if (o == Option.RoomA)
			roomText.text = "Room A";
		else if (o == Option.RoomB)
			roomText.text = "Room B";
	}
}
