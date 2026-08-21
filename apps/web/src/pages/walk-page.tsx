import { MoochIcon } from "../shared/ui/mooch-icon";
import { Button } from "../shared/ui/button";
import { SectionHeader } from "../shared/ui/section-header";

export function WalkPage() {
  return (
    <section className="mooch-page">
      <SectionHeader
        description="The most important flow in Mooch should stay simple: start, pick who joined, track, finish, share."
        title="Walk"
      />

      <section className="walk-flow-card">
        <div className="walk-flow-card__steps">
          <span><MoochIcon name="home" /> Home</span>
          <span><MoochIcon name="walk" /> Start Walk</span>
          <span><MoochIcon name="dog" /> Select Dog</span>
          <span>3... 2... 1...</span>
          <span><MoochIcon name="map" /> Tracking</span>
        </div>

        <div className="walk-flow-card__tracker">
          <h2>1.42 mi</h2>
          <p>27:13</p>
          <p>19:09 / mile</p>
          <div className="walk-flow-card__map">
            <MoochIcon name="map" />
            <span>Route map</span>
          </div>
          <Button type="button" variant="secondary">Pause</Button>
        </div>

        <p className="walk-flow-card__note">
          This should later ask one important question before tracking: did a dog join you, and if so which one or ones?
        </p>
      </section>
    </section>
  );
}
