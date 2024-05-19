using System.Text.Json;

namespace Celeste64;

/// <summary>
/// Creates a slight delay so the window looks OK before we load Assets
/// TODO: Would be nice if Foster could hide the Window till assets are ready.
/// </summary>
public class Startup : Scene
{
	private int loadDelay = 5;

	private void BeginGame()
	{
		// load assets
		Assets.Load();

		// load save file
		{
			var saveFile = Path.Join(App.UserPath, Save.FileName);

            Console.WriteLine("debug 0");
			if (File.Exists(saveFile))
				Save.Instance = Save.Deserialize(File.ReadAllText(saveFile)) ?? new();
			else
				Save.Instance = new();
            Console.WriteLine("debug 1");
			Save.Instance.SyncSettings();
            Console.WriteLine("debug 2");
		}

		// make sure the active language is ready for use,
		// since the save file may have loaded a different language than default.
		Language.Current.Use();

		// try to load controls, or overwrite with defaults if they don't exist
		{
			var controlsFile = Path.Join(App.UserPath, ControlsConfig.FileName);

			ControlsConfig? controls = null;
			if (File.Exists(controlsFile))
			{
				try
				{
					controls = JsonSerializer.Deserialize(File.ReadAllText(controlsFile), ControlsConfigContext.Default.ControlsConfig);
				}
				catch
				{
					controls = null;
				}
			}

			// create defaults if not found
			if (controls == null)
			{
				controls = ControlsConfig.Defaults;
				using var stream = File.Create(controlsFile);
				JsonSerializer.Serialize(stream, ControlsConfig.Defaults, ControlsConfigContext.Default.ControlsConfig);
				stream.Flush();
			}
			
			Controls.Load(controls);
		}
        Console.WriteLine("debug 3");

		// enter game
		//Assets.Levels[0].Enter(new AngledWipe());
		Game.Instance.Goto(new Transition()
		{
			Mode = Transition.Modes.Replace,
			Scene = () => new Titlescreen(),
			ToBlack = null,
			FromBlack = new AngledWipe(),
		});
        Console.WriteLine("debug 4");
	}

    public override void Update()
    {
		if (loadDelay > 0)
		{
			loadDelay--;
			if (loadDelay <= 0)
				BeginGame();
		}
    }

    public override void Render(Target target)
    {
		target.Clear(Color.Black);
    }
}
