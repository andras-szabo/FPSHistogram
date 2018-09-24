using RingBuffer;
using UnityEngine;

public class FpsHistogram : MonoBehaviour
{
	public bool updateEachFrame;

	public const int FPS_HISTOGRAM_COUNT = 60;
	public Material fpsHistogramMaterial;

	private RingBuffer<float> _pastFps = new RingBuffer<float>(FPS_HISTOGRAM_COUNT);
	private float[] _fpsArray = new float[FPS_HISTOGRAM_COUNT];

	private int _framesActuallyRendered;
	private int _lastFrameCount;
	private float _elapsedTime;

	private void Update()
	{
		_framesActuallyRendered++;
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime > 1f)
		{
			_elapsedTime -= 1f;
			ShowFpsHistogram(_framesActuallyRendered, 1f / Time.deltaTime);
			_lastFrameCount = _framesActuallyRendered;
			_framesActuallyRendered = 0;
		}
		else if (updateEachFrame)
		{
			ShowFpsHistogram(_lastFrameCount, 1f / Time.deltaTime);
		}
	}

	private void ShowFpsHistogram(int framesRenderedInPastSecond, float currentFrameTime)
	{
		_pastFps.Push(currentFrameTime);
		_pastFps.ToArray(_fpsArray);

		fpsHistogramMaterial.SetFloatArray("_FpsArray", _fpsArray);
		fpsHistogramMaterial.SetFloat("_CurrentFPS", (float)framesRenderedInPastSecond);
	}
}
