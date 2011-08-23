namespace Castle.Facilities.NHibernate
{
	/// <summary>
	/// Specifies the default session management strategy.
	/// </summary>
	public enum DefaultSessionLifeStyleOption : uint // internally, this uint corresponds to the order in which components are registered
	{
		/// <summary>
		/// Specifies that sessions should be opened and closed per transaction. This has the semantics
		/// that the session is kept per top transaction, unless the dependent transaction is forked, in
		/// which case, a new session is resolved to avoid sharing the session accross threads.
		/// </summary>
		SessionPerTransaction = 0,

		/// <summary>
		/// Specifies that sessions should be opened and closed per web request.
		/// </summary>
		SessionPerWebRequest = 1,

		/// <summary>
		/// Specifies that the session should be transiently registered.
		/// </summary>
		SessionTransient = 2
	}
}