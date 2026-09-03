import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";

import { createManualActivity } from "../features/activities/api";
import { fetchDogs } from "../features/dogs/api";
import type { DogSummary } from "../features/dogs/types";
import {
  beginStravaConnect,
  fetchStravaImports,
  fetchStravaStatus,
  syncStrava,
  type ImportedStravaActivity,
  type StravaStatus
} from "../features/integrations/api";
import { Button } from "../shared/ui/button";
import { MoochIcon } from "../shared/ui/mooch-icon";
import { SectionHeader } from "../shared/ui/section-header";

const emptyStatus: StravaStatus = {
  isConnected: false,
  pendingImportCount: 0
};

function formatRelativeDay(value?: string) {
  if (!value) {
    return "Not synced yet";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "Not synced yet";
  }

  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit"
  }).format(date);
}

function formatDuration(totalMinutes: number) {
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  if (hours === 0) {
    return `${minutes} min`;
  }

  return `${hours}h ${minutes}m`;
}

function toDateTimeLocalValue() {
  const now = new Date();
  now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
  return now.toISOString().slice(0, 16);
}

function toDateInputValue() {
  return toDateTimeLocalValue().slice(0, 10);
}

function toTimeInputValue() {
  return toDateTimeLocalValue().slice(11, 16);
}

function getDogInitials(name: string) {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
}

export function WalkPage() {
  const navigate = useNavigate();
  const [dogs, setDogs] = useState<DogSummary[]>([]);
  const [selectedDogIds, setSelectedDogIds] = useState<string[]>([]);
  const [title, setTitle] = useState("Neighborhood walk");
  const [notes, setNotes] = useState("");
  const [distanceMiles, setDistanceMiles] = useState("1.2");
  const [durationMinutes, setDurationMinutes] = useState("28");
  const [startedDate, setStartedDate] = useState(toDateInputValue);
  const [startedTime, setStartedTime] = useState(toTimeInputValue);
  const [manualError, setManualError] = useState<string | null>(null);
  const [manualFlash, setManualFlash] = useState<string | null>(null);
  const [isSavingManual, setIsSavingManual] = useState(false);

  const [status, setStatus] = useState<StravaStatus>(emptyStatus);
  const [imports, setImports] = useState<ImportedStravaActivity[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [flash, setFlash] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSyncing, setIsSyncing] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);

  async function loadStravaState() {
    setIsLoading(true);
    setError(null);

    try {
      const [nextStatus, nextImports] = await Promise.all([
        fetchStravaStatus(),
        fetchStravaImports()
      ]);

      setStatus(nextStatus);
      setImports(nextImports);
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : "Could not load Strava right now.");
    } finally {
      setIsLoading(false);
    }
  }

  async function loadDogs() {
    try {
      const nextDogs = await fetchDogs();
      setDogs(nextDogs);
      setSelectedDogIds((current) => {
        if (current.length > 0) {
          return current.filter((dogId) => nextDogs.some((dog) => dog.id === dogId));
        }

        return nextDogs[0] ? [nextDogs[0].id] : [];
      });
    } catch (loadError) {
      setManualError(loadError instanceof Error ? loadError.message : "Could not load your dogs.");
    }
  }

  useEffect(() => {
    void loadStravaState();
    void loadDogs();
  }, []);

  useEffect(() => {
    const url = new URL(window.location.href);
    const stravaStatus = url.searchParams.get("strava");

    if (!stravaStatus) {
      return;
    }

    const nextFlash = stravaStatus === "connected"
      ? "Strava connected. You can sync your walks, runs, and hikes now."
      : stravaStatus === "denied"
        ? "Strava connection was canceled."
        : "Strava connection did not finish cleanly. Try again.";

    setFlash(nextFlash);
    url.searchParams.delete("strava");
    window.history.replaceState({}, "", `${url.pathname}${url.search}${url.hash}`);
  }, []);

  const summaryText = useMemo(() => {
    if (!status.isConnected) {
      return "Log adventures by hand right now, then layer in imports when you are ready.";
    }

    if (status.pendingImportCount === 0) {
      return "Manual logging is ready, and Strava is connected whenever you want to pull in more adventures.";
    }

    return `${status.pendingImportCount} imported activit${status.pendingImportCount === 1 ? "y is" : "ies are"} waiting for a dog before they can show up in Mooch.`;
  }, [status]);

  function toggleDogSelection(dogId: string) {
    setSelectedDogIds((current) =>
      current.includes(dogId)
        ? current.filter((value) => value !== dogId)
        : [...current, dogId]);
  }

  async function handleManualSave() {
    setManualError(null);
    setManualFlash(null);
    setIsSavingManual(true);

    try {
      const saved = await createManualActivity({
        dogIds: selectedDogIds,
        title,
        notes,
        distanceMiles: Number(distanceMiles),
        durationMinutes: Number(durationMinutes),
        startedAtUtc: new Date(`${startedDate}T${startedTime}`).toISOString()
      });

      const selectedDogNames = dogs
        .filter((dog) => selectedDogIds.includes(dog.id))
        .map((dog) => dog.name);
      const dogLabel = selectedDogNames.length <= 1
        ? (selectedDogNames[0] ?? "Your dog")
        : `${selectedDogNames.slice(0, -1).join(", ")} and ${selectedDogNames[selectedDogNames.length - 1]}`;

      setManualFlash(`${dogLabel} just logged a ${saved.distanceMiles.toFixed(2)} mile adventure.`);
      setNotes("");
      await loadDogs();
    } catch (saveError) {
      setManualError(saveError instanceof Error ? saveError.message : "Could not save that activity.");
    } finally {
      setIsSavingManual(false);
    }
  }

  async function handleConnect() {
    setError(null);
    setFlash(null);
    setIsConnecting(true);

    try {
      await beginStravaConnect();
    } catch (connectError) {
      setError(connectError instanceof Error ? connectError.message : "Could not start the Strava connection.");
      setIsConnecting(false);
    }
  }

  async function handleSync() {
    setError(null);
    setFlash(null);
    setIsSyncing(true);

    try {
      const result = await syncStrava();
      await loadStravaState();
      setFlash(`${result.importedCount} imported, ${result.skippedCount} skipped. ${result.pendingImportCount} waiting for dog assignment.`);
    } catch (syncError) {
      setError(syncError instanceof Error ? syncError.message : "Could not sync Strava right now.");
    } finally {
      setIsSyncing(false);
    }
  }

  return (
    <section className="mooch-page">
      <div className="walk-page-header">
        <SectionHeader
          description="Add the details from your adventure."
          title="Log a walk"
        />
        <Button onClick={() => navigate("/app")} type="button" variant="secondary">
          <MoochIcon name="arrow-left" />
          <span>Back to activities</span>
        </Button>
      </div>

      <section className="walk-log-card">
        <section className="walk-log-section">
          <div className="walk-log-section__header">
            <span className="walk-log-section__icon">
              <MoochIcon name="paw" />
            </span>
            <div>
              <h2>Who came along?</h2>
              <p>Select every dog that joined this walk.</p>
            </div>
          </div>

          <div className="walk-dog-grid">
            {dogs.map((dog) => {
              const isSelected = selectedDogIds.includes(dog.id);

              return (
                <button
                  aria-pressed={isSelected}
                  className={["walk-dog-option", isSelected ? "walk-dog-option--selected" : ""].filter(Boolean).join(" ")}
                  key={dog.id}
                  onClick={() => toggleDogSelection(dog.id)}
                  type="button"
                >
                  {isSelected ? (
                    <span className="walk-dog-option__check">
                      <MoochIcon name="check" />
                    </span>
                  ) : null}
                  <span className="walk-dog-option__avatar">
                    {dog.avatarImage ? (
                      <img alt="" src={dog.avatarImage} />
                    ) : (
                      <span>{getDogInitials(dog.name)}</span>
                    )}
                  </span>
                  <strong>{dog.name}</strong>
                </button>
              );
            })}
          </div>
        </section>

        <section className="walk-log-section">
          <div className="walk-log-section__header">
            <span className="walk-log-section__icon">
              <MoochIcon name="walk" />
            </span>
            <div>
              <h2>Walk details</h2>
            </div>
          </div>

          <div className="walk-log-form">
            <label className="walk-field walk-field--full">
              <span>Title</span>
              <input className="walk-field__input" onChange={(event) => setTitle(event.target.value)} placeholder="Morning neighborhood loop" type="text" value={title} />
            </label>

            <label className="walk-field">
              <span>Distance</span>
              <span className="walk-field__with-unit">
                <input className="walk-field__input" inputMode="decimal" min="0" onChange={(event) => setDistanceMiles(event.target.value)} step="0.01" type="number" value={distanceMiles} />
                <span>miles</span>
              </span>
            </label>

            <label className="walk-field">
              <span>Duration</span>
              <span className="walk-field__with-unit">
                <input className="walk-field__input" inputMode="numeric" min="0" onChange={(event) => setDurationMinutes(event.target.value)} step="1" type="number" value={durationMinutes} />
                <span>minutes</span>
              </span>
            </label>

            <label className="walk-field">
              <span>Date</span>
              <span className="walk-field__icon-input">
                <input className="walk-field__input" onChange={(event) => setStartedDate(event.target.value)} type="date" value={startedDate} />
                <MoochIcon name="calendar" />
              </span>
            </label>

            <label className="walk-field">
              <span>Time</span>
              <span className="walk-field__icon-input">
                <input className="walk-field__input" onChange={(event) => setStartedTime(event.target.value)} type="time" value={startedTime} />
                <MoochIcon name="clock" />
              </span>
            </label>
          </div>
        </section>

        <section className="walk-log-section walk-log-section--notes">
          <div className="walk-log-section__header">
            <span className="walk-log-section__icon">
              <MoochIcon name="note" />
            </span>
            <div>
              <h2>Notes <span>(optional)</span></h2>
            </div>
          </div>

          <label className="walk-field walk-field--full">
            <textarea className="walk-field__textarea" onChange={(event) => setNotes(event.target.value)} placeholder="Anything memorable about this walk?" rows={4} value={notes} />
          </label>
        </section>

        <div className="walk-log-card__actions">
          <Button onClick={() => navigate("/app")} type="button" variant="text">
            Cancel
          </Button>
          <Button disabled={isSavingManual || dogs.length === 0 || selectedDogIds.length === 0} onClick={handleManualSave}>
            <MoochIcon name="save" />
            <span>{isSavingManual ? "Saving..." : "Log walk"}</span>
          </Button>
        </div>

        {manualFlash ? <p className="walk-sync-panel__flash">{manualFlash}</p> : null}
        {manualError ? <p className="walk-sync-panel__error">{manualError}</p> : null}
      </section>

      <section className="walk-sync-panel">
        <div className="walk-sync-panel__hero">
          <div className="walk-sync-panel__copy">
            <span className="walk-sync-panel__eyebrow">Desktop sync</span>
            <h2>Bring in Strava later</h2>
            <p>
              When you are ready, sync walks, runs, and hikes into Mooch, keep them private at first, and only attach them to the feed after you pick which doggo joined.
            </p>
            <div className="walk-sync-panel__actions">
              <Button disabled={isConnecting} onClick={handleConnect} variant={status.isConnected ? "secondary" : "primary"}>
                {status.isConnected ? "Reconnect Strava" : "Connect Strava"}
              </Button>
              <Button disabled={!status.isConnected || isSyncing || isLoading} onClick={handleSync} variant="secondary">
                {isSyncing ? "Syncing..." : "Sync now"}
              </Button>
            </div>
          </div>

          <div className="walk-sync-panel__status-card">
            <div className="walk-sync-panel__status-row">
              <span>Connection</span>
              <strong>{status.isConnected ? "Connected" : "Not connected"}</strong>
            </div>
            <div className="walk-sync-panel__status-row">
              <span>Pending imports</span>
              <strong>{status.pendingImportCount}</strong>
            </div>
            <div className="walk-sync-panel__status-row">
              <span>Latest import</span>
              <strong>{formatRelativeDay(status.lastImportedActivityAtUtc)}</strong>
            </div>
          </div>
        </div>

        {flash ? <p className="walk-sync-panel__flash">{flash}</p> : null}
        {error ? <p className="walk-sync-panel__error">{error}</p> : null}
      </section>

      <section className="walk-imports-card">
        <div className="walk-imports-card__header">
          <div>
            <h2>Imported activities</h2>
            <p>These are waiting for dog assignment before they become real Mooch adventures.</p>
          </div>
          {status.pendingImportCount > 0 ? (
            <span className="walk-imports-card__badge">{status.pendingImportCount} waiting</span>
          ) : null}
        </div>

        {isLoading ? <p className="walk-imports-card__empty">Loading your imports...</p> : null}

        {!isLoading && imports.length === 0 ? (
          <div className="walk-imports-card__empty-state">
            <h3>No imported activities yet</h3>
            <p>Once you connect Strava and run a sync, your recent walks, runs, and hikes will land here first.</p>
          </div>
        ) : null}

        {!isLoading && imports.length > 0 ? (
          <div className="walk-import-list">
            {imports.map((activity) => (
              <article className="walk-import-item" key={activity.importedActivityID}>
                <div className="walk-import-item__icon">
                  <MoochIcon name={activity.activityType.toLowerCase() === "walk" ? "walk" : "map"} />
                </div>
                <div className="walk-import-item__copy">
                  <div className="walk-import-item__title-row">
                    <h3>{activity.title}</h3>
                    {activity.requiresDogAssignment ? <span className="walk-import-item__pill">Needs dog</span> : null}
                  </div>
                  <p>
                    {activity.activityType} - {activity.distanceMiles.toFixed(2)} mi - {formatDuration(activity.durationMinutes)}
                  </p>
                </div>
                <time className="walk-import-item__time" dateTime={activity.startedAtUtc}>
                  {formatRelativeDay(activity.startedAtUtc)}
                </time>
              </article>
            ))}
          </div>
        ) : null}
      </section>
    </section>
  );
}
