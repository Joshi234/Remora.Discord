//
//  IntegrationApplication.cs
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

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;

namespace Remora.Discord.API.Objects
{
    /// <inheritdoc />
    public class IntegrationApplication : IIntegrationApplication
    {
        /// <inheritdoc />
        public Snowflake ID { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IImageHash? Icon { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public string Summary { get; }

        /// <inheritdoc />
        public Optional<IUser> Bot { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationApplication"/> class.
        /// </summary>
        /// <param name="id">The application ID.</param>
        /// <param name="name">The name of the application.</param>
        /// <param name="icon">The application's icon.</param>
        /// <param name="description">The description of the application.</param>
        /// <param name="summary">The summary of the application.</param>
        /// <param name="bot">The bot associated with this application, if any.</param>
        public IntegrationApplication
        (
            Snowflake id,
            string name,
            IImageHash? icon,
            string description,
            string summary,
            Optional<IUser> bot
        )
        {
            this.ID = id;
            this.Name = name;
            this.Icon = icon;
            this.Description = description;
            this.Summary = summary;
            this.Bot = bot;
        }
    }
}