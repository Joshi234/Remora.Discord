//
//  CachingDiscordRestGuildAPI.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Results;
using Remora.Discord.Caching.Services;
using Remora.Discord.Core;
using Remora.Discord.Rest;
using Remora.Discord.Rest.API;
using Remora.Discord.Rest.Results;

namespace Remora.Discord.Caching.API
{
    /// <summary>
    /// Implements a caching version of the channel API.
    /// </summary>
    public class CachingDiscordRestGuildAPI : DiscordRestGuildAPI
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheSettings _cacheSettings;

        /// <inheritdoc cref="DiscordRestGuildAPI" />
        public CachingDiscordRestGuildAPI
        (
            DiscordHttpClient discordHttpClient,
            IOptions<JsonSerializerOptions> jsonOptions,
            IMemoryCache memoryCache,
            CacheSettings cacheSettings
        )
            : base(discordHttpClient, jsonOptions)
        {
            _memoryCache = memoryCache;
            _cacheSettings = cacheSettings;
        }

        /// <inheritdoc />
        public override async Task<ICreateRestEntityResult<IGuild>> CreateGuildAsync
        (
            string name,
            Optional<string> region = default,
            Optional<Stream> icon = default,
            Optional<VerificationLevel> verificationLevel = default,
            Optional<MessageNotificationLevel> defaultMessageNotifications = default,
            Optional<ExplicitContentFilterLevel> explicitContentFilter = default,
            Optional<IReadOnlyList<IRole>> roles = default,
            Optional<IReadOnlyList<IPartialChannel>> channels = default,
            Optional<Snowflake> afkChannelID = default,
            Optional<TimeSpan> afkTimeout = default,
            Optional<Snowflake> systemChannelID = default,
            CancellationToken ct = default
        )
        {
            var createResult = await base.CreateGuildAsync
            (
                name,
                region,
                icon,
                verificationLevel,
                defaultMessageNotifications,
                explicitContentFilter,
                roles,
                channels,
                afkChannelID,
                afkTimeout,
                systemChannelID,
                ct
            );

            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            var guild = createResult.Entity;
            var key = KeyHelpers.CreateGuildCacheKey(guild.ID);
            _memoryCache.Set(key, guild, _cacheSettings.GetEntryOptions<IGuild>());

            return createResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IGuild>> GetGuildAsync
        (
            Snowflake guildID,
            Optional<bool> withCounts = default,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateGuildCacheKey(guildID);
            if (_memoryCache.TryGetValue<IGuild>(key, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IGuild>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildAsync(guildID, withCounts, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var guild = getResult.Entity;
            _memoryCache.Set(key, guild, _cacheSettings.GetEntryOptions<IGuild>());

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IGuildPreview>> GetGuildPreviewAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateGuildPreviewCacheKey(guildID);
            if (_memoryCache.TryGetValue<IGuildPreview>(key, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IGuildPreview>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildPreviewAsync(guildID, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var guildPreview = getResult.Entity;
            _memoryCache.Set(key, guildPreview, _cacheSettings.GetEntryOptions<IGuildPreview>());

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IModifyRestEntityResult<IGuild>> ModifyGuildAsync
        (
            Snowflake guildID,
            Optional<string> name = default,
            Optional<string?> region = default,
            Optional<VerificationLevel?> verificationLevel = default,
            Optional<MessageNotificationLevel?> defaultMessageNotifications = default,
            Optional<ExplicitContentFilterLevel?> explicitContentFilter = default,
            Optional<Snowflake?> afkChannelID = default,
            Optional<TimeSpan> afkTimeout = default,
            Optional<Stream?> icon = default,
            Optional<Snowflake> ownerID = default,
            Optional<Stream?> splash = default,
            Optional<Stream?> banner = default,
            Optional<Snowflake?> systemChannelID = default,
            Optional<Snowflake?> rulesChannelID = default,
            Optional<Snowflake?> publicUpdatesChannelID = default,
            Optional<string?> preferredLocale = default,
            CancellationToken ct = default
        )
        {
            var modifyResult = await base.ModifyGuildAsync
            (
                guildID,
                name,
                region,
                verificationLevel,
                defaultMessageNotifications,
                explicitContentFilter,
                afkChannelID,
                afkTimeout,
                icon,
                ownerID,
                splash,
                banner,
                systemChannelID,
                rulesChannelID,
                publicUpdatesChannelID,
                preferredLocale,
                ct
            );

            if (!modifyResult.IsSuccess)
            {
                return modifyResult;
            }

            var guild = modifyResult.Entity;
            var key = KeyHelpers.CreateGuildCacheKey(guild.ID);
            _memoryCache.Set(key, guild, _cacheSettings.GetEntryOptions<IGuild>());

            return modifyResult;
        }

        /// <inheritdoc />
        public override async Task<IDeleteRestEntityResult> DeleteGuildAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var deleteResult = await base.DeleteGuildAsync(guildID, ct);

            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }

            var key = KeyHelpers.CreateGuildCacheKey(guildID);
            _memoryCache.Remove(key);

            return deleteResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IReadOnlyList<IChannel>>> GetGuildChannelsAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var collectionKey = KeyHelpers.CreateGuildChannelsCacheKey(guildID);
            if (_memoryCache.TryGetValue<IReadOnlyList<IChannel>>(collectionKey, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IReadOnlyList<IChannel>>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildChannelsAsync(guildID, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var channels = getResult.Entity;
            _memoryCache.Set(collectionKey, channels, _cacheSettings.GetEntryOptions<IReadOnlyList<IChannel>>());

            foreach (var channel in channels)
            {
                var key = KeyHelpers.CreateChannelCacheKey(channel.ID);
                _memoryCache.Set(key, channel, _cacheSettings.GetEntryOptions<IChannel>());
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<ICreateRestEntityResult<IChannel>> CreateGuildChannelAsync
        (
            Snowflake guildID,
            string name,
            Optional<ChannelType> type = default,
            Optional<string> topic = default,
            Optional<int> bitrate = default,
            Optional<int> userLimit = default,
            Optional<int> rateLimitPerUser = default,
            Optional<int> position = default,
            Optional<IReadOnlyList<IPermissionOverwrite>> permissionOverwrites = default,
            Optional<Snowflake> parentID = default,
            Optional<bool> isNsfw = default,
            CancellationToken ct = default
        )
        {
            var createResult = await base.CreateGuildChannelAsync
            (
                guildID,
                name,
                type,
                topic,
                bitrate,
                userLimit,
                rateLimitPerUser,
                position,
                permissionOverwrites,
                parentID,
                isNsfw,
                ct
            );

            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            var guild = createResult.Entity;
            var key = KeyHelpers.CreateGuildCacheKey(guild.ID);
            _memoryCache.Set(key, guild, _cacheSettings.GetEntryOptions<IChannel>());

            return createResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IGuildMember>> GetGuildMemberAsync
        (
            Snowflake guildID,
            Snowflake userID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateGuildMemberKey(guildID, userID);
            if (_memoryCache.TryGetValue<IGuildMember>(key, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IGuildMember>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildMemberAsync(guildID, userID, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var guildMember = getResult.Entity;
            if (!guildMember.User.HasValue)
            {
                return getResult;
            }

            _memoryCache.Set(key, guildMember, _cacheSettings.GetEntryOptions<IGuildMember>());
            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IReadOnlyList<IGuildMember>>> ListGuildMembersAsync
        (
            Snowflake guildID,
            Optional<int> limit = default,
            Optional<Snowflake> after = default,
            CancellationToken ct = default
        )
        {
            var collectionKey = KeyHelpers.CreateGuildMembersKey(guildID, limit, after);
            if (_memoryCache.TryGetValue<IReadOnlyList<IGuildMember>>(collectionKey, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IReadOnlyList<IGuildMember>>.FromSuccess(cachedInstance);
            }

            var getResult = await base.ListGuildMembersAsync(guildID, limit, after, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var members = getResult.Entity;
            _memoryCache.Set(collectionKey, members, _cacheSettings.GetEntryOptions<IReadOnlyList<IGuildMember>>());

            foreach (var member in members)
            {
                if (!member.User.HasValue)
                {
                    continue;
                }

                var key = KeyHelpers.CreateGuildMemberKey(guildID, member.User.Value!.ID);
                _memoryCache.Set(key, member, _cacheSettings.GetEntryOptions<IGuildMember>());
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<ICreateRestEntityResult<IGuildMember?>> AddGuildMemberAsync
        (
            Snowflake guildID,
            Snowflake userID,
            string accessToken,
            Optional<string> nickname = default,
            Optional<IReadOnlyList<Snowflake>> roles = default,
            Optional<bool> isMuted = default,
            Optional<bool> isDeafened = default,
            CancellationToken ct = default
        )
        {
            var getResult = await base.AddGuildMemberAsync
            (
                guildID,
                userID,
                accessToken,
                nickname,
                roles,
                isMuted,
                isDeafened,
                ct
            );

            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var member = getResult.Entity;
            if (member is null)
            {
                return getResult;
            }

            var key = KeyHelpers.CreateGuildMemberKey(guildID, userID);
            _memoryCache.Set(key, member, _cacheSettings.GetEntryOptions<IGuildMember>());
            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IReadOnlyList<IBan>>> GetGuildBansAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var collectionKey = KeyHelpers.CreateGuildBansCacheKey(guildID);
            if (_memoryCache.TryGetValue<IReadOnlyList<IBan>>(collectionKey, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IReadOnlyList<IBan>>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildBansAsync(guildID, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var bans = getResult.Entity;
            _memoryCache.Set(collectionKey, bans, _cacheSettings.GetEntryOptions<IReadOnlyList<IBan>>());

            foreach (var ban in bans)
            {
                var key = KeyHelpers.CreateGuildBanCacheKey(guildID, ban.User.ID);
                _memoryCache.Set(key, ban, _cacheSettings.GetEntryOptions<IBan>());
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IBan>> GetGuildBanAsync
        (
            Snowflake guildID,
            Snowflake userID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateGuildBanCacheKey(guildID, userID);
            if (_memoryCache.TryGetValue<IBan>(key, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IBan>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildBanAsync(guildID, userID, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var ban = getResult.Entity;
            _memoryCache.Set(key, ban, _cacheSettings.GetEntryOptions<IBan>());

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IDeleteRestEntityResult> RemoveGuildBanAsync
        (
            Snowflake guildID,
            Snowflake userID,
            CancellationToken ct = default
        )
        {
            var deleteResult = await base.RemoveGuildBanAsync(guildID, userID, ct);
            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }

            var key = KeyHelpers.CreateGuildBanCacheKey(guildID, userID);
            _memoryCache.Remove(key);

            return deleteResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IReadOnlyList<IRole>>> GetGuildRolesAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var collectionKey = KeyHelpers.CreateGuildRolesCacheKey(guildID);
            if (_memoryCache.TryGetValue<IReadOnlyList<IRole>>(collectionKey, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IReadOnlyList<IRole>>.FromSuccess(cachedInstance);
            }

            var getRoles = await base.GetGuildRolesAsync(guildID, ct);
            if (!getRoles.IsSuccess)
            {
                return getRoles;
            }

            var roles = getRoles.Entity;
            _memoryCache.Set(collectionKey, roles, _cacheSettings.GetEntryOptions<IReadOnlyList<IRole>>());

            foreach (var role in roles)
            {
                var key = KeyHelpers.CreateGuildRoleCacheKey(guildID, role.ID);
                _memoryCache.Set(key, role, _cacheSettings.GetEntryOptions<IRole>());
            }

            return getRoles;
        }

        /// <inheritdoc />
        public override async Task<ICreateRestEntityResult<IRole>> CreateGuildRoleAsync
        (
            Snowflake guildID,
            Optional<string> name = default,
            Optional<IDiscordPermissionSet> permissions = default,
            Optional<Color> colour = default,
            Optional<bool> isHoisted = default,
            Optional<bool> isMentionable = default,
            CancellationToken ct = default
        )
        {
            var createResult = await base.CreateGuildRoleAsync
            (
                guildID,
                name,
                permissions,
                colour,
                isHoisted,
                isMentionable,
                ct
            );

            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            var role = createResult.Entity;
            var key = KeyHelpers.CreateGuildRoleCacheKey(guildID, role.ID);
            _memoryCache.Set(key, role, _cacheSettings.GetEntryOptions<IRole>());

            return createResult;
        }

        /// <inheritdoc />
        public override async Task<IModifyRestEntityResult<IReadOnlyList<IRole>>> ModifyGuildRolePositionsAsync
        (
            Snowflake guildID,
            IReadOnlyList<(Snowflake RoleID, Optional<int?> Position)> modifiedPositions,
            CancellationToken ct = default
        )
        {
            var modifyResult = await base.ModifyGuildRolePositionsAsync(guildID, modifiedPositions, ct);

            if (!modifyResult.IsSuccess)
            {
                return modifyResult;
            }

            var roles = modifyResult.Entity;
            var collectionKey = KeyHelpers.CreateGuildRolesCacheKey(guildID);
            _memoryCache.Set(collectionKey, roles, _cacheSettings.GetEntryOptions<IReadOnlyList<IRole>>());

            foreach (var role in roles)
            {
                var key = KeyHelpers.CreateGuildRoleCacheKey(guildID, role.ID);
                _memoryCache.Set(key, role, _cacheSettings.GetEntryOptions<IRole>());
            }

            return modifyResult;
        }

        /// <inheritdoc />
        public override async Task<IModifyRestEntityResult<IRole>> ModifyGuildRoleAsync
        (
            Snowflake guildID,
            Snowflake roleID,
            Optional<string?> name = default,
            Optional<IDiscordPermissionSet?> permissions = default,
            Optional<Color?> colour = default,
            Optional<bool?> isHoisted = default,
            Optional<bool?> isMentionable = default,
            CancellationToken ct = default
        )
        {
            var modifyResult = await base.ModifyGuildRoleAsync
            (
                guildID,
                roleID,
                name,
                permissions,
                colour,
                isHoisted,
                isMentionable,
                ct
            );

            if (!modifyResult.IsSuccess)
            {
                return modifyResult;
            }

            var role = modifyResult.Entity;
            var key = KeyHelpers.CreateGuildRoleCacheKey(guildID, roleID);
            _memoryCache.Set(key, role, _cacheSettings.GetEntryOptions<IRole>());

            return modifyResult;
        }

        /// <inheritdoc />
        public override async Task<IDeleteRestEntityResult> DeleteGuildRoleAsync
        (
            Snowflake guildId,
            Snowflake roleID,
            CancellationToken ct = default
        )
        {
            var deleteResult = await base.DeleteGuildRoleAsync(guildId, roleID, ct);
            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }

            var key = KeyHelpers.CreateGuildRoleCacheKey(guildId, roleID);
            _memoryCache.Remove(key);

            return deleteResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IReadOnlyList<IVoiceRegion>>> GetGuildVoiceRegionsAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var collectionKey = KeyHelpers.CreateGuildVoiceRegionsCacheKey(guildID);
            if (_memoryCache.TryGetValue<IReadOnlyList<IVoiceRegion>>(collectionKey, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IReadOnlyList<IVoiceRegion>>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildVoiceRegionsAsync(guildID, ct);

            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var voiceRegions = getResult.Entity;
            _memoryCache.Set
            (
                collectionKey,
                voiceRegions,
                _cacheSettings.GetEntryOptions<IReadOnlyList<IVoiceRegion>>()
            );

            foreach (var voiceRegion in voiceRegions)
            {
                var key = KeyHelpers.CreateGuildVoiceRegionCacheKey(guildID, voiceRegion.ID);
                _memoryCache.Set(key, voiceRegion, _cacheSettings.GetEntryOptions<IVoiceRegion>());
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IReadOnlyList<IInvite>>> GetGuildInvitesAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var collectionKey = KeyHelpers.CreateGuildInvitesCacheKey(guildID);
            if (_memoryCache.TryGetValue<IReadOnlyList<IInvite>>(collectionKey, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IReadOnlyList<IInvite>>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildInvitesAsync(guildID, ct);

            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var invites = getResult.Entity;
            _memoryCache.Set(collectionKey, invites, _cacheSettings.GetEntryOptions<IReadOnlyList<IInvite>>());

            foreach (var invite in invites)
            {
                var key = KeyHelpers.CreateInviteCacheKey(invite.Code);
                _memoryCache.Set(key, invite, _cacheSettings.GetEntryOptions<IInvite>());
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IReadOnlyList<IIntegration>>> GetGuildIntegrationsAsync
        (
            Snowflake guildID,
            Optional<bool> includeApplications = default,
            CancellationToken ct = default
        )
        {
            var collectionKey = KeyHelpers.CreateGuildIntegrationsCacheKey(guildID);
            if (_memoryCache.TryGetValue<IReadOnlyList<IIntegration>>(collectionKey, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IReadOnlyList<IIntegration>>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildIntegrationsAsync(guildID, includeApplications, ct);

            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var integrations = getResult.Entity;
            _memoryCache.Set
            (
                collectionKey,
                integrations,
                _cacheSettings.GetEntryOptions<IReadOnlyList<IIntegration>>()
            );

            foreach (var integration in integrations)
            {
                var key = KeyHelpers.CreateGuildIntegrationCacheKey(guildID, integration.ID);
                _memoryCache.Set(key, integration, _cacheSettings.GetEntryOptions<IIntegration>());
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IGuildWidget>> GetGuildWidgetSettingsAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateGuildWidgetSettingsCacheKey(guildID);
            if (_memoryCache.TryGetValue<IGuildWidget>(key, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IGuildWidget>.FromSuccess(cachedInstance);
            }

            var getResult = await base.GetGuildWidgetSettingsAsync(guildID, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var widget = getResult.Entity;
            _memoryCache.Set(key, widget, _cacheSettings.GetEntryOptions<IGuildWidget>());

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<IModifyRestEntityResult<IGuildWidget>> ModifyGuildWidgetAsync
        (
            Snowflake guildID,
            Optional<bool> isEnabled = default,
            Optional<Snowflake?> channelID = default,
            CancellationToken ct = default
        )
        {
            var modifyResult = await base.ModifyGuildWidgetAsync(guildID, isEnabled, channelID, ct);
            if (!modifyResult.IsSuccess)
            {
                return modifyResult;
            }

            var key = KeyHelpers.CreateGuildWidgetSettingsCacheKey(guildID);
            var widget = modifyResult.Entity;
            _memoryCache.Set(key, widget, _cacheSettings.GetEntryOptions<IGuildWidget>());

            return modifyResult;
        }
    }
}