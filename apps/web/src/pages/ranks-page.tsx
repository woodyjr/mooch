import { Button } from "../shared/ui/button";
import { MoochIcon } from "../shared/ui/mooch-icon";
import { SectionHeader } from "../shared/ui/section-header";

const leaderboard = [
  { rank: 1, dog: "Winston", breed: "Golden Retriever", miles: 18.4 },
  { rank: 2, dog: "Luna", breed: "Australian Shepherd", miles: 16.7 },
  { rank: 12, dog: "Mooch · You", breed: "Up 4 places this week", miles: 8.7 }
];

export function RanksPage() {
  return (
    <section className="mooch-page">
      <SectionHeader
        action={<Button type="button" variant="secondary">Cincinnati v</Button>}
        description="See how Mooch stacks up this week."
        title="Leaderboards"
      />

      <section className="leaderboard-card">
        <div className="leaderboard-card__head">
          <span>Rank</span>
          <span>Dog</span>
          <span>Miles sniffed</span>
        </div>

        {leaderboard.map((entry) => (
          <div className="leaderboard-row" key={entry.rank}>
            <strong>{entry.rank}</strong>
            <div className="leaderboard-row__dog">
              <span className="leaderboard-row__mark">
                <MoochIcon name="dog" />
              </span>
              <div>
                <h3>{entry.dog}</h3>
                <p>{entry.breed}</p>
              </div>
            </div>
            <strong>{entry.miles}</strong>
          </div>
        ))}
      </section>
    </section>
  );
}
