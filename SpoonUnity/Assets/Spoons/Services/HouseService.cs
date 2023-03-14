
using System.Collections.Generic;

public class HouseService
{
	public List<HouseBehaviour> houses = new List<HouseBehaviour>();
	
	public void RegisterHouse(HouseBehaviour houseBehaviour)
	{
		houses.Add(houseBehaviour);
	}

	public void RemoveHouse(HouseBehaviour houseBehaviour)
	{
		houses.Remove(houseBehaviour);
	}
}
