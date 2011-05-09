namespace Castle.Facilities.NHibernate
{
	/// <summary>
	/// Specifies the default session management strategy.
	/// </summary>
	public enum DefaultSessionLifeStyleOption
	{
		/// <summary>
		/// Specifies that sessions should be opened and closed per transaction.
		/// </summary>
		SessionPerTransaction,

		/// <summary>
		/// Specifies that sessions should be opened and closed per web request.
		/// </summary>
		SessionPerWebRequest
	}
}