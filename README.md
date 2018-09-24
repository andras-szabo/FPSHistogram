# FPSHistogram
A lightweight frames-per-second display that does the heavy lifting in a shader.

The "FpsHistogram" monobehaviour counts the number of frames actually rendered in the past second, and it also tries to
measure the frame rate based on the time elapsed between consecutive Update() calls. It uses a ring buffer to keep track
of the history of frame times. The histogram is visualised entirely by a custom shader, which also displays the number
of frames rendered in the past second.
