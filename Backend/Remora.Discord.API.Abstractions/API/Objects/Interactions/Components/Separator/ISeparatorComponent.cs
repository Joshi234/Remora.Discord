//
//  ISeparatorComponent.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
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

using JetBrains.Annotations;
using Remora.Rest.Core;

namespace Remora.Discord.API.Abstractions.Objects;

/// <summary>
/// Represents a component that vertically separates other components.
/// </summary>
[PublicAPI]
public interface ISeparatorComponent : IMessageComponent, IPartialSeparatorComponent
{
    /// <summary>
    /// Gets whether this component acts as a divider, and will render as a line.
    /// </summary>
    new Optional<bool> IsDivider { get; }

    /// <summary>
    /// Gets the spacing of this separator.
    /// </summary>
    new Optional<SeparatorSpacingSize> Spacing { get; }

    /// <inheritdoc/>
    Optional<bool> IPartialSeparatorComponent.IsDivider => this.IsDivider;

    /// <inheritdoc/>
    Optional<SeparatorSpacingSize> IPartialSeparatorComponent.Spacing => this.Spacing;
}
