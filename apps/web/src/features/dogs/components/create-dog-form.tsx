import { type FormEvent, useState } from "react";

import type { CreateDogInput } from "../api";
import { Button } from "../../../shared/ui/button";
import { TextAreaField, TextField } from "../../../shared/ui/field";

type CreateDogFormProps = {
  ctaLabel?: string;
  helperText?: string;
  isSaving?: boolean;
  onSubmit: (input: CreateDogInput) => Promise<unknown>;
};

export function CreateDogForm({
  ctaLabel = "Add best friend",
  helperText,
  isSaving = false,
  onSubmit
}: CreateDogFormProps) {
  const [name, setName] = useState("");
  const [breed, setBreed] = useState("");
  const [birthDate, setBirthDate] = useState("");
  const [weightPounds, setWeightPounds] = useState("");
  const [avatarImage, setAvatarImage] = useState("");
  const [bio, setBio] = useState("");
  const [coOwnerEmail, setCoOwnerEmail] = useState("");
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    if (!name.trim()) {
      setError("Give your furry friend a name so we know who joined the adventure.");
      return;
    }

    try {
      await onSubmit({
        name,
        breed,
        birthDate,
        weightPounds: weightPounds.trim() ? Number(weightPounds) : undefined,
        bio,
        avatarImage,
        coOwnerEmail
      });

      setName("");
      setBreed("");
      setBirthDate("");
      setWeightPounds("");
      setAvatarImage("");
      setBio("");
      setCoOwnerEmail("");
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : "Could not save your dog right now. Please try again later.");
    }
  }

  return (
    <form className="dog-form" onSubmit={handleSubmit}>
      <div className="dog-form__grid">
        <TextField
          id="dog-name"
          label="Dog name"
          onChange={(event) => setName(event.target.value)}
          placeholder="Mochi"
          value={name}
        />

        <TextField
          id="dog-breed"
          label="Breed"
          onChange={(event) => setBreed(event.target.value)}
          placeholder="Lab mix"
          value={breed}
        />

        <TextField
          id="dog-birthday"
          label="Birthday"
          onChange={(event) => setBirthDate(event.target.value)}
          type="date"
          value={birthDate}
        />

        <TextField
          id="dog-weight"
          inputMode="decimal"
          label="Weight (lbs)"
          onChange={(event) => setWeightPounds(event.target.value)}
          placeholder="42"
          value={weightPounds}
        />
      </div>

      <TextField
        id="dog-photo"
        label="Photo URL"
        onChange={(event) => setAvatarImage(event.target.value)}
        value={avatarImage}
      />

      <TextAreaField
        id="dog-bio"
        label="Quick note"
        onChange={(event) => setBio(event.target.value)}
        placeholder="Must find a stick and greet every dog in sight."
        rows={3}
        value={bio}
      />

      <div className="dog-form__invite-block">
        <TextField
          id="dog-co-owner-email"
          label="Invite another owner"
          onChange={(event) => setCoOwnerEmail(event.target.value)}
          placeholder="partner@example.com"
          type="email"
          value={coOwnerEmail}
        />
        <p className="dog-form__helper">
          Optional. If someone else shares this doggo with you, invite them now so they can join the same profile instead of creating a duplicate dog.
        </p>
      </div>

      {helperText && <p className="dog-form__helper">{helperText}</p>}
      {error && <p className="form-error form-error--light">{error}</p>}

      <Button disabled={isSaving} type="submit">
        {isSaving ? "Saving..." : ctaLabel}
      </Button>
    </form>
  );
}
