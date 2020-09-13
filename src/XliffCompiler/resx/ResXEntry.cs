// <copyright file="ResXEntry.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.ResX
{
    using System;

    public class ResXEntry : IComparable
    {
        public string Id { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public int CompareTo(object obj)
        {
            if (obj is ResXEntry)
            {
                return this.Id.CompareTo((obj as ResXEntry).Id);
            }

            return this.Id.CompareTo(obj.ToString());
        }
    }
}