import type { DogSummary } from "../types";

type DogListProps = {
  dogs: DogSummary[];
};

export function DogList({ dogs }: DogListProps) {
  if (dogs.length === 0) {
    return <p>No dogs yet.</p>;
  }

  return (
    <div className="dog-grid">
      {dogs.map((dog) => (
        <article className="dog-card" key={dog.id}>
          <div className="dog-card__badge">
            {dog.streakDays > 0 ? `${dog.streakDays} day streak` : "Ready for the next outing"}
          </div>
          <h3>{dog.name}</h3>
          <p className="dog-card__breed">{dog.breed}</p>
          <p>{dog.bio}</p>
        </article>
      ))}
    </div>
  );
}
