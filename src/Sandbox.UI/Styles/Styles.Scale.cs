namespace Sandbox.UI
{
	public partial class Styles
	{
		// NOTE: ApplyScale is disabled for now as it requires Length.Scale() method
		// which isn't implemented yet. This method is used for UI scaling/DPI support.
		// The critical style lifecycle fixes (From/Add methods) are in Styles.From.cs
		// and work independently.
		
		/*
		public void ApplyScale( float scale )
		{
			// Scale implementation requires Length.Scale() method
			// to be added to the Length struct
		}
		*/

		internal bool CalcVisible()
		{
			if ( Display.HasValue && Display.Value == DisplayMode.None ) return false;
			if ( Opacity <= 0.0f ) return false;

			return true;
		}

		private void Scale( ShadowList? shadows, float amount )
		{
			if ( shadows == null ) return;

			for ( int i = 0; i < shadows.Count; i++ )
			{
				shadows[i] = shadows[i].Scale( amount );
			}
		}

		private void Scale( ref PanelTransform? tx, float amount )
		{
			if ( tx == null ) return;

			tx = tx.Value.GetScaled( amount );
		}
	}
}
