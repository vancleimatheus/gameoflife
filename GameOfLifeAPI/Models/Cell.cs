namespace GameOfLifeAPI.Models;

public class Cell(bool initialState)
{
    public bool State { get; private set; } = initialState;
    public bool OldState { get; set; } = initialState;
    public List<Cell> Neighbours { get; set; } = [];

    public void NextState()
    {
        var liveNeighbours = Neighbours.Count(n => n.OldState);

        //Exact 3 neighbours alive, it will always be alive
        if (liveNeighbours == 3)
            State = true;

        //Less than 2 or more than 3 neighbours alive, the cell will die
        if (liveNeighbours < 2 || liveNeighbours > 3)
            State = false;

        //If none of those previous conditions were met state remains unchanged
    }

    public void Link(Cell neighbour)
    {
        Neighbours.Add(neighbour);
        neighbour.Neighbours.Add(this);
    }

    public void CommitState() => OldState = State;

}