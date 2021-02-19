using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TerraFX.Interop;

namespace ProjectEstrada
{
    unsafe class DirectXApp
    {
		// game states
		bool hasStarted;                        // true iff the DirectXApp was started completely

		// game options
		bool showFPS;                           // true if and only if FPS information should be printed to the screen. Default: true; can be toggled via F1

		// timer
		Timer timer;                           // high-precision timer
		static double dt;                        // constant game update rate
		static double maxSkipFrames;             // constant maximum of frames to skip in the update loop (important to not stall the system on slower computers)

		// application window
		protected static HINSTANCE appInstance;            // handle to an instance of the application
		protected static Window appWindow;                // the application window (i.e. game window)

		// game state
		protected bool isPaused;                          // true iff the game is paused

		// stats
		protected int fps;                                // frames per second
		protected double mspf;                            // milliseconds per frame

		// DirectX Graphics
		protected Direct3D d3d;                // pointer to the Direct3D class
		protected Direct2D d2d;                // pointer to the Direct2D class


		public DirectXApp(HINSTANCE hInstance)
        {
			appInstance = hInstance;
			appWindow = null;
			isPaused = true;
			timer = null;
			fps = 0;
			mspf = 0.0;
			dt = 1000 / 240.0;
			maxSkipFrames = 10;
			hasStarted = false;
			showFPS = true;
			d3d = null;
			d2d = null;
		}
		~DirectXApp()
		{
			shutdown();
		}

		/// <summary>
		/// Create Core App Components
		/// </summary>
		bool init()
		{
			// create the game timer
			try { timer = new Timer(); }
			catch (std::runtime_error)
			{
				throw new Exception("The high-precision timer could not be started!");
			}

			// create the application window
			try { appWindow = new Window(); }
			catch
			{
				throw new Exception("DirectXApp was unable to create the main window!");
			}

			// initialize Direct3D
			try { d3d = new Direct3D(this); }
			catch
			{
				throw new Exception("DirectXApp was unable to initialize Direct3D!");
			}

			// initialize Direct2D
			try { d2d = new Direct2D(this); }
			catch (std::runtime_error)
			{
				throw new Exception("DirectXApp was unable to initialize Direct2D!");
			}

			// log and return success
			hasStarted = true;
			util::ServiceLocator::getFileLogger()->print<util::SeverityType::info>("The DirectX application initialization was successful.");
			return true;
		}

		void shutdown(object expected)
		{
			if (d2d)
				delete d2d;

			if (d3d)
				delete d3d;

			if (appWindow)
				delete appWindow;

			if (timer)
				delete timer;
		}

		/// <summary>
		/// Game loop
		/// </summary>
		int run()
		{
			// reset (start) the timer
			timer.reset();

			double accumulatedTime = 0.0;       // stores the time accumulated by the rendered
			int nLoops = 0;                     // the number of completed loops while updating the game

			// enter main event loop
			bool continueRunning = true;
			while (continueRunning)
			{
				// let the timer tick
				timer.tick();

				if (!isPaused)
				{
					// compute fps
					if (!calculateFrameStatistics())
						return -1;

					// acquire input

					// accumulate the elapsed time since the last frame
					accumulatedTime += timer.getDeltaTime();

					// now update the game logic with fixed dt as often as possible
					nLoops = 0;
					while (accumulatedTime >= dt && nLoops < maxSkipFrames)
					{
						result = update(dt);
						if (!result.isValid())
							return result;
						accumulatedTime -= dt;
						nLoops++;
					}

					// peek into the future and generate the output
					result = render(accumulatedTime / dt);
					if (!result.isValid())
						return result;
				}
			}

#if NDEBUG
			util::ServiceLocator::getFileLogger()->print<util::SeverityType::info>("Leaving the game loop...");
#endif
			return 0;
		}

		/// <summary>
		/// Input
		/// </summary>
		void onKeyDown(WPARAM wParam, LPARAM lParam)
		{
			switch (wParam)
			{
				case VK_F1:
					showFPS = !showFPS;
					break;

				case VK_ESCAPE:
					PostMessage(appWindow->mainWindow, WM_CLOSE, 0, 0);
					break;

				default: break;

			}
		}

		/// <summary>
		/// Resizing
		/// </summary>
		bool onResize()
		{
#if NDEBUG
			util::ServiceLocator::getFileLogger()->print<util::SeverityType::warning>("The window was resized. The game graphics must be updated!");
#endif
			if (!d3d->onResize().wasSuccessful())
				throw new Exception("Unable to resize Direct3D resources!");

			// return success
			return true;
		}

		static int nFrames;                 // number of frames seen
		static double elapsedTime;          // time since last call

		/// <summary>
		/// Frame Statistics
		/// </summary>
		bool calculateFrameStatistics()
		{
			nFrames++;

			// compute average statistics over one second
			if ((timer.getTotalTime() - elapsedTime) >= 1.0)
			{
				// set fps and mspf
				fps = nFrames;
				mspf = 1000.0 / (double)fps;

				if (showFPS)
				{
					// create FPS information text layout
					std::wostringstream outFPS;
					outFPS.precision(6);
					outFPS << "FPS: " << fps << std::endl;
					outFPS << "mSPF: " << mspf << std::endl;

					if (FAILED(d2d->writeFactory->CreateTextLayout(outFPS.str().c_str(), (UInt32)outFPS.str().size(), d2d->textFormatFPS.Get(), (float)appWindow->clientWidth, (float)appWindow->clientHeight, &d2d->textLayoutFPS)))
						throw new Exception("Critical error: Failed to create the text layout for FPS information!");
				}

				// reset
				nFrames = 0;
				elapsedTime += 1.0;
			}

			// return success
			return true;
		}
	}
}
