import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

import { fetchDogOwnerInvites, fetchDogs, inviteDogOwner } from "../features/dogs/api";
import { Badge } from "../shared/ui/badge";
import { Button } from "../shared/ui/button";
import { Card } from "../shared/ui/card";
import { EmptyState } from "../shared/ui/empty-state";
import { TextField } from "../shared/ui/field";
import { MoochIcon } from "../shared/ui/mooch-icon";
import { SectionHeader } from "../shared/ui/section-header";
import { Tabs } from "../shared/ui/tabs";

export function DogsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [searchParams, setSearchParams] = useSearchParams();
  const { data: dogs = [], isLoading, error } = useQuery({
    queryKey: ["dogs"],
    queryFn: fetchDogs
  });
  const [selectedDogId, setSelectedDogId] = useState<string | null>(null);
  const [inviteEmail, setInviteEmail] = useState("");
  const [inviteError, setInviteError] = useState("");

  const requestedDogId = searchParams.get("selected");

  useEffect(() => {
    if (!dogs.length) {
      return;
    }

    if (requestedDogId && dogs.some((dog) => dog.id === requestedDogId)) {
      setSelectedDogId(requestedDogId);
      setSearchParams((currentParams) => {
        const nextParams = new URLSearchParams(currentParams);
        nextParams.delete("selected");
        return nextParams;
      }, { replace: true });
      return;
    }

    setSelectedDogId((currentDogId) => currentDogId ?? dogs[0].id);
  }, [dogs, requestedDogId, setSearchParams]);

  const selectedDog = useMemo(() => {
    if (!dogs.length) {
      return null;
    }

    return dogs.find((dog) => dog.id === selectedDogId) ?? dogs[0];
  }, [dogs, selectedDogId]);

  const { data: ownerInvites = [], isLoading: isInvitesLoading } = useQuery({
    queryKey: ["dog-owner-invites", selectedDog?.id],
    queryFn: () => fetchDogOwnerInvites(selectedDog!.id),
    enabled: !!selectedDog
  });

  const inviteOwnerMutation = useMutation({
    mutationFn: async (email: string) => inviteDogOwner(selectedDog!.id, email),
    onSuccess: async () => {
      setInviteEmail("");
      setInviteError("");
      await queryClient.invalidateQueries({ queryKey: ["dog-owner-invites", selectedDog?.id] });
      await queryClient.invalidateQueries({ queryKey: ["dogs"] });
    },
    onError: (submitError) => {
      setInviteError(submitError instanceof Error ? submitError.message : "Could not send that co-owner invite right now.");
    }
  });

  async function handleInviteSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setInviteError("");

    if (!inviteEmail.trim()) {
      setInviteError("Add an email address to invite another owner.");
      return;
    }

    await inviteOwnerMutation.mutateAsync(inviteEmail);
  }

  return (
    <section className="mooch-page">
      <Button
        className="my-dogs__add-button"
        onClick={() => navigate("/app/dogs/new")}
        type="button"
        variant="primary"
      >
        <span>
          <MoochIcon name="plus" />
        </span>
        <span>Add Dog</span>
      </Button>

      <SectionHeader
        description="Switch between each dog you have set up and keep their profile current."
        title="My Dogs"
      />

      {isLoading && <p className="mooch-page__status">Loading your dogs...</p>}
      {error && <p className="mooch-page__status">Could not load your dogs right now.</p>}

      {!isLoading && !error && dogs.length > 0 && (
        <>
          <div className="dog-switcher">
            <Tabs
              items={dogs.map((dog) => ({ label: dog.name, value: dog.id }))}
              onChange={setSelectedDogId}
              value={selectedDog?.id ?? dogs[0].id}
            />
          </div>

          {selectedDog && (
            <section className="dog-profile-card">
              <div className="dog-profile-card__hero">
                <div className="dog-profile-card__avatar">
                  {selectedDog.avatarImage ? (
                    <img alt={selectedDog.name} className="hero-card__avatar-image" src={selectedDog.avatarImage} />
                  ) : (
                    <MoochIcon name="dog" />
                  )}
                </div>

                <div className="dog-profile-card__copy">
                  <p className="eyebrow">Primary identity</p>
                  <h2>{selectedDog.name}</h2>
                  <p>{selectedDog.breed}</p>
                  <p>{selectedDog.bio}</p>
                </div>
              </div>

              <div className="metric-grid">
                <article className="metric-card">
                  <p>Miles sniffed</p>
                  <h3>312</h3>
                  <span>all time</span>
                </article>
                <article className="metric-card">
                  <p>Adventures</p>
                  <h3>147</h3>
                  <span>all time</span>
                </article>
                <article className="metric-card">
                  <p>Outside time</p>
                  <h3>42</h3>
                  <span>adventure time</span>
                </article>
                <article className="metric-card">
                  <p>Owners</p>
                  <h3>{selectedDog.ownerCount}</h3>
                  <span>{selectedDog.isPrimaryOwner ? "you manage this profile" : "shared dog profile"}</span>
                </article>
              </div>

              <div className="owner-panel">
                <div className="owner-panel__header">
                  <div>
                    <h3>Owners & invites</h3>
                    <p>Invite a spouse, partner, or walker to the same dog profile so nobody creates a duplicate dog.</p>
                  </div>
                </div>

                <form className="owner-panel__form" onSubmit={(event) => void handleInviteSubmit(event)}>
                  <TextField
                    id="dog-owner-email"
                    label="Co-owner email"
                    onChange={(event) => setInviteEmail(event.target.value)}
                    placeholder="partner@example.com"
                    type="email"
                    value={inviteEmail}
                  />
                  <Button disabled={inviteOwnerMutation.isPending} type="submit" variant="secondary">
                    {inviteOwnerMutation.isPending ? "Sending..." : "Invite owner"}
                  </Button>
                </form>

                {inviteError && <p className="form-error form-error--light">{inviteError}</p>}

                <div className="owner-panel__summary">
                  <Badge variant="gold">
                    {selectedDog.ownerCount} owner{selectedDog.ownerCount === 1 ? "" : "s"} connected
                  </Badge>
                  {selectedDog.isPrimaryOwner && (
                    <Badge variant="accent">Primary owner</Badge>
                  )}
                </div>

                <div className="owner-panel__invite-list">
                  {isInvitesLoading && <p className="mooch-page__status">Loading invites...</p>}
                  {!isInvitesLoading && ownerInvites.length === 0 && (
                    <EmptyState
                      message="As soon as you invite a spouse, partner, or walker, the shared dog profile history will show up here."
                      title="No co-owner invites yet"
                    />
                  )}
                  {ownerInvites.map((invite) => (
                    <div className="owner-invite-row" key={invite.id}>
                      <div>
                        <strong>{invite.email}</strong>
                        <p>Sent {new Date(invite.createdDateUtc).toLocaleDateString("en-US", { month: "short", day: "numeric" })}</p>
                      </div>
                      <Badge className="owner-invite-row__status" variant="gold">{invite.status}</Badge>
                    </div>
                  ))}
                </div>
              </div>

              <div className="achievement-card">
                <h3>Achievements</h3>
                <ul className="achievement-list">
                  <li>10 walk streak</li>
                  <li>Top 10 Cincinnati</li>
                  <li>100 Mile Club</li>
                </ul>
              </div>
            </section>
          )}
        </>
      )}

      {!isLoading && !error && dogs.length === 0 && (
        <Card className="panel">
          <EmptyState
            action={<Button onClick={() => navigate("/app/dogs/new")}>Add your first dog</Button>}
            eyebrow="No dogs yet"
            message="Add your first dog to give Mooch a profile to center around."
            title="Start your pack"
          />
        </Card>
      )}
    </section>
  );
}
