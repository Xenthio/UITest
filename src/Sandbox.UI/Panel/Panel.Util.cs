using System.Threading;

namespace Sandbox.UI;

/// <summary>
/// Utility methods and properties for Panel.
/// Ported from s&box engine/Sandbox.Engine/Systems/UI/Panel/Panel.Util.cs
/// </summary>
public partial class Panel
{
	/// <summary>
	/// Can be used to store random data without sub-classing the panel.
	/// </summary>
	public object? UserData { get; set; }

	CancellationTokenSource? _deleteTokenSource;

	/// <summary>
	/// Get a token that is cancelled when the panel is deleted.
	/// Useful for async operations that should stop when the panel is removed.
	/// </summary>
	public CancellationToken DeletionToken
	{
		get
		{
			if (IsDeleting || !IsValid())
				return CancellationToken.None;

			_deleteTokenSource ??= new CancellationTokenSource();
			return _deleteTokenSource.Token;
		}
	}

	/// <summary>
	/// Invoke a method after a delay. If the panel is deleted before this delay, the method will not be called.
	/// </summary>
	public async void Invoke(float seconds, Action action)
	{
		await Task.Delay(TimeSpan.FromSeconds(seconds));
		if (!this.IsValid()) return;

		try
		{
			action();
		}
		catch (Exception e)
		{
			Log.Error($"Error in Invoke: {e}");
		}
	}

	Dictionary<string, CancellationTokenSource>? invokes;

	/// <summary>
	/// Invoke a method after a delay. If the panel is deleted before this delay, the method will not be called.
	/// If the invoke is called while the old one is waiting, the old one will be cancelled.
	/// </summary>
	public async void InvokeOnce(string name, float seconds, Action action)
	{
		CancelInvoke(name);

		var tokenSource = new CancellationTokenSource();
		invokes ??= new Dictionary<string, CancellationTokenSource>();
		invokes[name] = tokenSource;

		try
		{
			await Task.Delay(TimeSpan.FromSeconds(seconds), tokenSource.Token);
		}
		catch (TaskCanceledException)
		{
			return;
		}

		if (!this.IsValid()) return;

		if (tokenSource.IsCancellationRequested)
			return;

		invokes.Remove(name);

		try
		{
			action();
		}
		catch (Exception e)
		{
			Log.Error($"Error in InvokeOnce: {e}");
		}
	}

	/// <summary>
	/// Cancel a named invocation.
	/// </summary>
	public void CancelInvoke(string name)
	{
		if (invokes != null && invokes.Remove(name, out var cts))
		{
			cts.Cancel();
			cts.Dispose();
		}
	}
}
