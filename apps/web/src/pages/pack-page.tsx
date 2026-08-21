import { useDeferredValue, useState } from "react";

import { Button } from "../shared/ui/button";
import { Card } from "../shared/ui/card";
import { EmptyState } from "../shared/ui/empty-state";
import { TextField } from "../shared/ui/field";
import { MoochIcon } from "../shared/ui/mooch-icon";
import { SectionHeader } from "../shared/ui/section-header";

type PackMember = {
  dog: string;
  human: string;
  status: "accepted" | "suggested";
  note: string;
};

const packMembers: PackMember[] = [
  { dog: "Luna", human: "Emily", status: "accepted", note: "Trail buddy · active 1h ago" },
  { dog: "Winston", human: "Avery", status: "accepted", note: "25 Mile Week badge" },
  { dog: "Charlie", human: "Nina", status: "suggested", note: "Also walks in Cincinnati" },
  { dog: "Maple", human: "Jordan", status: "suggested", note: "Shares your neighborhood loop" },
  { dog: "Scout", human: "Priya", status: "suggested", note: "Mutual dog friends" },
  { dog: "Mabel", human: "Chris", status: "accepted", note: "Local park regular" },
  { dog: "Otis", human: "Dana", status: "suggested", note: "Close by and very active" },
  { dog: "Piper", human: "Sofia", status: "accepted", note: "Morning walk crew" }
];

function filterMembers(members: PackMember[], search: string) {
  const term = search.trim().toLowerCase();
  if (!term) {
    return members;
  }

  return members.filter((member) =>
    member.dog.toLowerCase().includes(term) ||
    member.human.toLowerCase().includes(term)
  );
}

export function PackPage() {
  const [search, setSearch] = useState("");
  const deferredSearch = useDeferredValue(search);
  const visibleMembers = filterMembers(packMembers, deferredSearch);
  const acceptedFriends = visibleMembers.filter((member) => member.status === "accepted");
  const suggestedFriends = visibleMembers.filter((member) => member.status === "suggested");

  return (
    <section className="mooch-page">
      <SectionHeader
        description="Friends who have accepted your request, plus suggested dogs to invite next."
        title="The Pack"
      />

      <Card className="pack-search-card">
        <TextField
          id="pack-search"
          label="Search by dog name or human name"
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search Luna, Emily, Winston..."
          value={search}
        />
      </Card>

      <section className="section-block">
        <SectionHeader
          description={`${acceptedFriends.length} accepted friend${acceptedFriends.length === 1 ? "" : "s"}`}
          headingLevel="h2"
          title="Your pack"
        />

        <div className="pack-grid">
          {acceptedFriends.map((friend) => (
            <article className="pack-tile" key={`${friend.dog}-${friend.human}`}>
              <div className="pack-tile__avatar">
                <MoochIcon name="dog" />
              </div>
              <div className="pack-tile__body">
                <h3>{friend.dog}</h3>
                <p>{friend.human}</p>
                <span>{friend.note}</span>
              </div>
              <Button size="sm" type="button" variant="secondary">View</Button>
            </article>
          ))}

          {acceptedFriends.length === 0 && (
            <EmptyState
              message="Try a different dog or human name, or keep growing your pack."
              title="No accepted friends match that search yet"
            />
          )}
        </div>
      </section>

      <section className="section-block">
        <SectionHeader
          description={`${suggestedFriends.length} dog${suggestedFriends.length === 1 ? "" : "s"} you can invite`}
          headingLevel="h2"
          title="Suggested friends"
        />

        <div className="pack-grid">
          {suggestedFriends.map((friend) => (
            <article className="pack-tile pack-tile--suggested" key={`${friend.dog}-${friend.human}`}>
              <div className="pack-tile__avatar">
                <MoochIcon name="plus" />
              </div>
              <div className="pack-tile__body">
                <h3>{friend.dog}</h3>
                <p>{friend.human}</p>
                <span>{friend.note}</span>
              </div>
              <Button size="sm" type="button" variant="secondary">Invite</Button>
            </article>
          ))}

          {suggestedFriends.length === 0 && (
            <EmptyState
              message="You have cleared the current list. More local dog recommendations can show up here later."
              title="No suggestions match that search right now"
            />
          )}
        </div>
      </section>
    </section>
  );
}
