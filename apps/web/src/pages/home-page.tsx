import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { Navigate } from "react-router-dom";

import { useAuth } from "../features/auth/auth-context";
import { fetchDogDashboard, fetchDogs } from "../features/dogs/api";
import { Badge } from "../shared/ui/badge";
import { Button } from "../shared/ui/button";
import { EmptyState } from "../shared/ui/empty-state";
import { MoochIcon } from "../shared/ui/mooch-icon";
import { SectionHeader } from "../shared/ui/section-header";
import { Tabs } from "../shared/ui/tabs";

function getGreeting(date: Date): string {
  const hour = date.getHours();

  if (hour < 12) {
    return "Good morning";
  }

  if (hour < 18) {
    return "Good afternoon";
  }

  return "Good evening";
}

function formatActivityMeta(startedAt: string, distanceMiles: number, durationMinutes: number): string {
  const activityDate = new Date(startedAt);
  const now = new Date();
  const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const startOfActivityDay = new Date(activityDate.getFullYear(), activityDate.getMonth(), activityDate.getDate());
  const diffDays = Math.round((startOfToday.getTime() - startOfActivityDay.getTime()) / 86_400_000);

  const dayLabel =
    diffDays === 0
      ? "Today"
      : diffDays === 1
        ? "Yesterday"
        : activityDate.toLocaleDateString("en-US", { weekday: "long" });

  const paceMinutes = durationMinutes / Math.max(distanceMiles, 0.01);
  const paceWholeMinutes = Math.floor(paceMinutes);
  const paceSeconds = Math.round((paceMinutes - paceWholeMinutes) * 60)
    .toString()
    .padStart(2, "0");

  return `${dayLabel} · ${distanceMiles.toFixed(1)} mi · ${durationMinutes} min · ${paceWholeMinutes}:${paceSeconds} / mi`;
}

function formatOutsideTime(totalMinutes: number): string {
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  if (hours === 0) {
    return `${minutes}m`;
  }

  return `${hours}h ${minutes}m`;
}

export function HomePage() {
  const { user } = useAuth();
  const { data: dogs = [], isLoading, error } = useQuery({
    queryKey: ["dogs"],
    queryFn: fetchDogs
  });
  const [selectedDogId, setSelectedDogId] = useState<string | null>(null);

  const hasDogs = dogs.length > 0;
  const selectedDog = useMemo(() => {
    if (!dogs.length) {
      return null;
    }

    return dogs.find((dog) => dog.id === selectedDogId) ?? dogs[0];
  }, [dogs, selectedDogId]);

  const {
    data: dashboard,
    isLoading: isDashboardLoading,
    error: dashboardError
  } = useQuery({
    queryKey: ["dog-dashboard", selectedDog?.id],
    queryFn: () => fetchDogDashboard(selectedDog!.id),
    enabled: !!selectedDog
  });

  if (!isLoading && !error && !hasDogs) {
    return <Navigate replace to="/app/onboarding" />;
  }

  const now = new Date();
  const greeting = getGreeting(now);
  const formattedDate = new Intl.DateTimeFormat("en-US", {
    weekday: "long",
    month: "long",
    day: "numeric"
  }).format(now);

  const dogName = dashboard?.name ?? selectedDog?.name ?? "Mooch";
  const weeklyGoal = dashboard?.weeklyGoalMiles ?? 10;
  const weeklyMiles = dashboard?.weeklyMiles ?? 0;
  const weeklyProgress = weeklyGoal > 0 ? Math.round((weeklyMiles / weeklyGoal) * 100) : 0;
  const remainingMiles = Math.max(0, weeklyGoal - weeklyMiles);

  return (
    <section className="mooch-page">
      <div className="mooch-page__hero">
        <p className="mooch-page__date">{formattedDate}</p>
        <h1>{greeting}, {user?.displayName?.split(" ", 2)[0] ?? "friend"}</h1>
        <p>{dogName} is {remainingMiles.toFixed(1)} miles away from this week&apos;s goal.</p>
      </div>

      {isLoading && <p className="mooch-page__status">Loading your best friends...</p>}
      {error && <p className="mooch-page__status">Could not load your dogs right now.</p>}

      {hasDogs && selectedDog && (
        <>
          {dogs.length > 1 && (
            <Tabs
              items={dogs.map((dog) => ({ label: dog.name, value: dog.id }))}
              onChange={setSelectedDogId}
              value={selectedDog.id}
            />
          )}

          {isDashboardLoading && <p className="mooch-page__status">Loading {selectedDog.name}&apos;s dashboard...</p>}
          {dashboardError && <p className="mooch-page__status">Could not load the latest dog stats right now.</p>}

          {dashboard && (
            <>
              <section className="hero-card">
                <div className="hero-card__dog">
                  <div className="hero-card__avatar">
                    {selectedDog.avatarImage ? (
                      <img alt={selectedDog.name} className="hero-card__avatar-image" src={selectedDog.avatarImage} />
                    ) : (
                      <MoochIcon name="dog" />
                    )}
                  </div>

                  <div className="hero-card__summary">
                    <div className="hero-card__title-row">
                      <h2>{dashboard.name}&apos;s week</h2>
                      <div className="streak-tooltip">
                        <button
                          aria-describedby="streak-tooltip-copy"
                          className="hero-card__streak"
                          type="button"
                        >
                          <Badge variant="gold">
                            Streak: {dashboard.streakDays} day{dashboard.streakDays === 1 ? "" : "s"}
                          </Badge>
                        </button>
                        <div className="streak-tooltip__bubble" id="streak-tooltip-copy" role="tooltip">
                          Counts consecutive days your dog logged at least one walk.
                        </div>
                      </div>
                    </div>
                    <p>{dashboard.weeklyMiles.toFixed(1)} of {dashboard.weeklyGoalMiles.toFixed(0)} miles complete</p>
                    <div className="hero-card__goal-row">
                      <span>Weekly goal</span>
                      <strong>{weeklyProgress}%</strong>
                    </div>
                    <div className="hero-card__progress">
                      <div className="hero-card__progress-fill" style={{ width: `${Math.min(100, weeklyProgress)}%` }} />
                    </div>
                  </div>
                </div>

                <Button className="hero-card__button" type="button">
                  Start a walk
                </Button>
              </section>

              <section className="metric-grid">
                <article className="metric-card">
                  <p>Miles sniffed</p>
                  <h3>{dashboard.weeklyMiles.toFixed(1)}</h3>
                  <span>this week</span>
                </article>
                <article className="metric-card">
                  <p>Adventures</p>
                  <h3>{dashboard.weeklyAdventures}</h3>
                  <span>this week</span>
                </article>
                <article className="metric-card">
                  <p>Outside time</p>
                  <h3>{formatOutsideTime(dashboard.weeklyOutsideMinutes)}</h3>
                  <span>this week</span>
                </article>
                <article className="metric-card">
                  <p>Pack rank</p>
                  <h3>#{dashboard.packRank}</h3>
                  <span>{dashboard.ownerCount > 1 ? `${dashboard.ownerCount} owners on this profile` : "solo dog profile"}</span>
                </article>
              </section>

              <section className="section-block">
                <SectionHeader
                  action={<Button type="button" variant="text">View activity</Button>}
                  description={`${dashboard.name}'s latest activity`}
                  headingLevel="h2"
                  title="Recent adventures"
                />

                <div className="activity-list">
                  {dashboard.recentActivities.length === 0 && (
                    <EmptyState
                      message="Once your dog logs walks, runs, or hikes, their latest adventures will show up here."
                      title="No adventures yet"
                    />
                  )}

                  {dashboard.recentActivities.map((activity) => {
                    const iconName =
                      activity.title.toLowerCase().includes("trail")
                        ? "trail"
                        : activity.title.toLowerCase().includes("evening")
                          ? "moon"
                          : "walk";

                    return (
                      <article className="activity-card" key={activity.id}>
                        <div className="activity-card__icon">
                          <MoochIcon name={iconName} />
                        </div>
                        <div className="activity-card__body">
                          <div className="activity-card__title-row">
                            <h3>{activity.title}</h3>
                          </div>
                          <p>{formatActivityMeta(activity.startedAt, activity.distanceMiles, activity.durationMinutes)}</p>
                        </div>
                        <Button type="button" variant="secondary">View</Button>
                      </article>
                    );
                  })}
                </div>
              </section>

              <section className="challenge-card">
                <SectionHeader
                  description={dashboard.weeklyChallenge.name}
                  headingLevel="h2"
                  title="Weekly challenge"
                />

                <div className="challenge-card__body">
                  <div className="challenge-card__summary">
                    <div className="challenge-card__medal">
                      <MoochIcon name="medal" />
                    </div>
                    <div className="challenge-card__copy">
                      <Badge variant="gold">On track</Badge>
                      <h3>{dashboard.weeklyChallenge.remainingMiles.toFixed(1)} miles to go</h3>
                      <p>Finish before Sunday to keep your dog in the club.</p>
                    </div>
                  </div>
                </div>

                <div className="hero-card__progress">
                  <div className="hero-card__progress-fill" style={{ width: `${Math.min(100, weeklyProgress)}%` }} />
                </div>

                <Button className="challenge-card__button" type="button" variant="secondary">
                  See challenges
                </Button>
              </section>
            </>
          )}
        </>
      )}
    </section>
  );
}
