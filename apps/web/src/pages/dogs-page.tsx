import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useState, type ChangeEvent, type FormEvent } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

import { deleteDog, fetchDogOwnerInvites, fetchDogs, inviteDogOwner, updateDog, uploadDogAvatar } from "../features/dogs/api";
import { Badge } from "../shared/ui/badge";
import { Button } from "../shared/ui/button";
import { Card } from "../shared/ui/card";
import { EmptyState } from "../shared/ui/empty-state";
import { TextAreaField, TextField } from "../shared/ui/field";
import { MoochIcon } from "../shared/ui/mooch-icon";
import { SectionHeader } from "../shared/ui/section-header";
import { Tabs } from "../shared/ui/tabs";

function getDogInitials(name: string) {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
}

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
  const [profileError, setProfileError] = useState("");
  const [isEditingProfile, setIsEditingProfile] = useState(false);
  const [editName, setEditName] = useState("");
  const [editBreed, setEditBreed] = useState("");
  const [editBirthDate, setEditBirthDate] = useState("");
  const [editWeightPounds, setEditWeightPounds] = useState("");
  const [editBio, setEditBio] = useState("");

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

    setSelectedDogId((currentDogId) => {
      if (currentDogId && dogs.some((dog) => dog.id === currentDogId)) {
        return currentDogId;
      }

      return dogs[0].id;
    });
  }, [dogs, requestedDogId, setSearchParams]);

  const selectedDog = useMemo(() => {
    if (!dogs.length) {
      return null;
    }

    return dogs.find((dog) => dog.id === selectedDogId) ?? dogs[0];
  }, [dogs, selectedDogId]);

  useEffect(() => {
    if (!selectedDog) {
      setIsEditingProfile(false);
      setEditName("");
      setEditBreed("");
      setEditBirthDate("");
      setEditWeightPounds("");
      setEditBio("");
      return;
    }

    setIsEditingProfile(false);
    setEditName(selectedDog.name);
    setEditBreed(selectedDog.breed === "Breed not added yet" ? "" : selectedDog.breed);
    setEditBirthDate(selectedDog.birthDate ?? "");
    setEditWeightPounds(selectedDog.weightPounds?.toString() ?? "");
    setEditBio(selectedDog.bio === "No notes yet." ? "" : selectedDog.bio);
  }, [selectedDog]);

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

  const updateDogMutation = useMutation({
    mutationFn: updateDog,
    onSuccess: async (dog) => {
      setProfileError("");
      setIsEditingProfile(false);
      setSelectedDogId(dog.id);
      await queryClient.invalidateQueries({ queryKey: ["dogs"] });
      await queryClient.invalidateQueries({ queryKey: ["dog-dashboard", dog.id] });
    },
    onError: (submitError) => {
      setProfileError(submitError instanceof Error ? submitError.message : "Could not update that dog profile right now.");
    }
  });

  const uploadAvatarMutation = useMutation({
    mutationFn: async ({ dogId, file }: { dogId: string; file: File }) => uploadDogAvatar(dogId, file),
    onSuccess: async (dog) => {
      setProfileError("");
      setSelectedDogId(dog.id);
      await queryClient.invalidateQueries({ queryKey: ["dogs"] });
      await queryClient.invalidateQueries({ queryKey: ["dog-dashboard", dog.id] });
    },
    onError: (submitError) => {
      setProfileError(submitError instanceof Error ? submitError.message : "Could not upload that dog photo right now.");
    }
  });

  const deleteDogMutation = useMutation({
    mutationFn: deleteDog,
    onSuccess: async () => {
      setProfileError("");
      setSelectedDogId(null);
      await queryClient.invalidateQueries({ queryKey: ["dogs"] });
      await queryClient.invalidateQueries({ queryKey: ["dog-dashboard"] });
      await queryClient.invalidateQueries({ queryKey: ["pending-dog-owner-invites"] });
    },
    onError: (submitError) => {
      setProfileError(submitError instanceof Error ? submitError.message : "Could not remove that dog from your pack right now.");
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

  function resetEditForm() {
    if (!selectedDog) {
      return;
    }

    setProfileError("");
    setIsEditingProfile(false);
    setEditName(selectedDog.name);
    setEditBreed(selectedDog.breed === "Breed not added yet" ? "" : selectedDog.breed);
    setEditBirthDate(selectedDog.birthDate ?? "");
    setEditWeightPounds(selectedDog.weightPounds?.toString() ?? "");
    setEditBio(selectedDog.bio === "No notes yet." ? "" : selectedDog.bio);
  }

  async function handleProfileSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedDog) {
      return;
    }

    setProfileError("");

    if (!editName.trim()) {
      setProfileError("Name is required.");
      return;
    }

    try {
      await updateDogMutation.mutateAsync({
        dogId: selectedDog.id,
        name: editName,
        breed: editBreed,
        birthDate: editBirthDate,
        weightPounds: editWeightPounds.trim() ? Number(editWeightPounds) : undefined,
        bio: editBio
      });
    } catch {
      // The mutation's onError already sets profileError.
    }
  }

  async function handleAvatarChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";

    if (!selectedDog || !file) {
      return;
    }

    await uploadAvatarMutation.mutateAsync({ dogId: selectedDog.id, file });
  }

  async function handleDeleteDog() {
    if (!selectedDog) {
      return;
    }

    const warning = selectedDog.isPrimaryOwner
      ? `Are you sure you want to remove ${selectedDog.name}?\n\nThis will delete all data associated with ${selectedDog.name}. This action cannot be undone.`
      : `Are you sure you want to remove ${selectedDog.name} from your saved dogs?\n\nThe shared dog profile will remain for other owners.`;

    const confirmed = window.confirm(warning);

    if (!confirmed) {
      return;
    }

    try {
      await deleteDogMutation.mutateAsync(selectedDog.id);
    } catch {
      // The mutation's onError already sets profileError.
    }
  }

  return (
    <section className="mooch-page dogs-page">
      <SectionHeader
        action={(
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
        )}
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
                <div className="dog-profile-card__identity">
                  <div className="dog-profile-card__avatar">
                    {selectedDog.avatarImage ? (
                      <img alt={selectedDog.name} className="dog-profile-card__avatar-image" src={selectedDog.avatarImage} />
                    ) : (
                      <span>{getDogInitials(selectedDog.name)}</span>
                    )}
                  </div>

                  <div className="dog-profile-card__copy">
                    <p className="eyebrow">{selectedDog.isPrimaryOwner ? "Primary identity" : "Shared dog"}</p>
                    <h2>{selectedDog.name}</h2>
                    <p>{selectedDog.breed}</p>
                    <p>{selectedDog.bio}</p>
                  </div>
                </div>

                <div className="dog-profile-card__actions">
                  <Button
                    disabled={updateDogMutation.isPending}
                    onClick={() => {
                      setProfileError("");
                      setIsEditingProfile((current) => !current);
                    }}
                    type="button"
                    variant="secondary"
                  >
                    <MoochIcon name="settings" />
                    <span>{isEditingProfile ? "Close edit" : "Edit profile"}</span>
                  </Button>
                  <label className="ui-button ui-button--secondary ui-button--md dog-profile-card__upload" htmlFor={`dog-avatar-${selectedDog.id}`}>
                    <MoochIcon name="upload" />
                    <span>{uploadAvatarMutation.isPending ? "Uploading..." : "Upload photo"}</span>
                  </label>
                  <input
                    accept="image/gif,image/jpeg,image/png,image/webp"
                    disabled={uploadAvatarMutation.isPending}
                    id={`dog-avatar-${selectedDog.id}`}
                    onChange={(event) => void handleAvatarChange(event)}
                    type="file"
                  />
                  <Button
                    disabled={deleteDogMutation.isPending}
                    onClick={() => void handleDeleteDog()}
                    type="button"
                    variant="text"
                  >
                    <MoochIcon name="trash" />
                    <span>{deleteDogMutation.isPending ? "Removing..." : selectedDog.isPrimaryOwner ? "Remove dog" : "Remove from saved dogs"}</span>
                  </Button>
                </div>
              </div>

              {isEditingProfile && (
                <form className="dog-profile-edit" onSubmit={(event) => void handleProfileSubmit(event)}>
                  <div className="dog-profile-edit__grid">
                    <TextField
                      disabled={updateDogMutation.isPending}
                      id="edit-dog-name"
                      label="Dog name"
                      onChange={(event) => setEditName(event.target.value)}
                      value={editName}
                    />

                    <TextField
                      disabled={updateDogMutation.isPending}
                      id="edit-dog-breed"
                      label="Breed"
                      onChange={(event) => setEditBreed(event.target.value)}
                      placeholder="Lab mix"
                      value={editBreed}
                    />

                    <TextField
                      disabled={updateDogMutation.isPending}
                      id="edit-dog-birthday"
                      label="Birthday"
                      onChange={(event) => setEditBirthDate(event.target.value)}
                      type="date"
                      value={editBirthDate}
                    />

                    <TextField
                      disabled={updateDogMutation.isPending}
                      id="edit-dog-weight"
                      inputMode="decimal"
                      label="Weight (lbs)"
                      onChange={(event) => setEditWeightPounds(event.target.value)}
                      placeholder="42"
                      value={editWeightPounds}
                    />
                  </div>

                  <TextAreaField
                    disabled={updateDogMutation.isPending}
                    id="edit-dog-bio"
                    label="Quick note"
                    onChange={(event) => setEditBio(event.target.value)}
                    placeholder="Must find a stick and greet every dog in sight."
                    rows={3}
                    value={editBio}
                  />

                  <div className="dog-profile-edit__actions">
                    <Button
                      disabled={updateDogMutation.isPending}
                      onClick={resetEditForm}
                      type="button"
                      variant="text"
                    >
                      Cancel
                    </Button>
                    <Button disabled={updateDogMutation.isPending} type="submit">
                      {updateDogMutation.isPending ? "Saving..." : "Save changes"}
                    </Button>
                  </div>
                </form>
              )}

              {profileError && <p className="form-error form-error--light dog-profile-card__error">{profileError}</p>}

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
