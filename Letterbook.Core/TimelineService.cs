using System.Security.Claims;
using Letterbook.Core.Adapters;
using Letterbook.Core.Exceptions;
using Letterbook.Core.Extensions;
using Letterbook.Core.Models;
using Medo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Letterbook.Core;

public class TimelineService : IAuthzTimelineService, ITimelineService
{
	private ILogger<TimelineService> _logger;
	private CoreOptions _options;
	private IFeedsAdapter _feeds;
	private IAccountProfileAdapter _profileAdapter;
	private readonly IAuthorizationService _authz;
	private IEnumerable<Claim>? _claims;

	public TimelineService(ILogger<TimelineService> logger, IOptions<CoreOptions> options, IFeedsAdapter feeds, IAccountProfileAdapter profileAdapter, IAuthorizationService authz)
	{
		_logger = logger;
		_options = options.Value;
		_feeds = feeds;
		_profileAdapter = profileAdapter;
		_authz = authz;
	}

	/// <inheritdoc />
	public async Task HandlePublish(Post post)
	{
		// TODO: account for moderation conditions (blocks, etc)
		post.Audience = NormalizeAudience(post);
		_feeds.AddToTimeline(post);
		await _feeds.Commit();
	}

	/// <inheritdoc />
	public async Task HandleShare(Post post, Profile sharedBy)
	{
		var boostedBy = post.SharesCollection.Last();
		_feeds.AddToTimeline(post, boostedBy);
		await _feeds.Commit();
	}

	/// <inheritdoc />
	public async Task HandleUpdate(Post post, Post oldPost)
	{
		var removed = NormalizeAudience(oldPost);
		removed.ExceptWith(NormalizeAudience(post));
		var added = NormalizeAudience(post);
		added.ExceptWith(NormalizeAudience(oldPost));

		await _feeds.Start();
		if (added.Count != 0)
		{
			post.Audience = added;
			_feeds.AddToTimeline(post);
		}

		if (removed.Count != 0) await _feeds.RemoveFromTimelines(post, removed);

		if (post.Preview != oldPost.Preview) await _feeds.UpdateTimeline(post);
		await _feeds.Commit();
	}

	/// <inheritdoc />
	public async Task HandleDelete(Post note)
	{
		// TODO: Also handle deleted boosts
		await _feeds.RemoveFromAllTimelines(note);
	}


	/// <inheritdoc />
	public async Task<IEnumerable<Post>> GetFeed(Uuid7 profileId, DateTimeOffset begin, int limit = 40)
	{
		// TODO(moderation): Account for moderation conditions (block, mute, etc)
		var recipient = await _profileAdapter.LookupProfile(profileId);
		return recipient != null
			? _feeds.GetTimelineEntries(recipient.Audiences, begin, limit).ToList()
			: throw CoreException.MissingData("Couldn't lookup Profile to load Feed", typeof(Guid), profileId);
	}

	/// <summary>
	/// Get the audience entries for the addressed recipients, plus followers/public/local, if applicable
	/// </summary>
	/// <param name="post"></param>
	/// <returns></returns>
	private HashSet<Audience> NormalizeAudience(Post post)
	{
		var result = new HashSet<Audience>();
		result.UnionWith(post.Audience);
		result.UnionWith(post.AddressedTo.Where(
			m => m.Subject.HasLocalAuthority(_options)).Select(m => Audience.FromMention(m.Subject)));

		// The "public audience" would be equivalent to Mastodon's federated global feed
		// That's not the same thing as putting posts into follower's feeds.
		// This ensures we include public posts in the followers audience in case the sender doesn't specify it
		if (!result.Contains(Audience.Public)) return result;
		result.UnionWith(post.Creators.Select(Audience.Followers));

		return result;
	}

	public IAuthzTimelineService As(IEnumerable<Claim> claims)
	{
		_claims = claims;
		return this;
	}
}