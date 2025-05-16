namespace SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;

/// <summary>
/// Marker interface.
/// Value objects do not have identity.
/// They are immutable.
/// Compared by using their attributes or properties.
/// Generally need context perhaps from a parent.
/// Improve ubiquitous language.
/// Help to eliminate primitive obsession.
/// </summary>
public interface IValueObject;