﻿namespace Letterbook.Core.Models;

public interface IObjectRef
{
    public Uri Id { get; set; }
    public string? LocalId { get; set; }
    public Uri Authority { get; set; }
    public HashSet<Profile> Creators { get; set; }
}