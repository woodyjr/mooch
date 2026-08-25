import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { Navigate } from "react-router-dom";

import { useAuth } from "../features/auth/auth-context";
import { fetchDogDashboard, fetchDogs } from "../features/dogs/api";
import { Button } from "../shared/ui/button";
import { EmptyState } from "../shared/ui/empty-state";
import { MoochIcon } from "../shared/ui/mooch-icon";

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

function formatOutsideTime(totalMinutes: number): string {
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  if (hours === 0) {
    return `${minutes}m`;
  }

  return `${hours}h ${minutes}m`;
}

function formatActivityDate(startedAt: string): string {
  const activityDate = new Date(startedAt);
  const now = new Date();
  const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const startOfActivityDay = new Date(activityDate.getFullYear(), activityDate.getMonth(), activityDate.getDate());
  const diffDays = Math.round((startOfToday.getTime() - startOfActivityDay.getTime()) / 86_400_000);

  if (diffDays === 0) {
    return "Today";
  }

  if (diffDays === 1) {
    return "Yesterday";
  }

  return activityDate.toLocaleDateString("en-US", { month: "short", day: "numeric" });
}

function formatClockTime(startedAt: string): string {
  return new Date(startedAt).toLocaleTimeString("en-US", {
    hour: "numeric",
    minute: "2-digit"
  });
}

function getDogInitials(name: string): string {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
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

  const greeting = getGreeting(new Date());
  const dogName = dashboard?.name ?? selectedDog?.name ?? "Mooch";
  const weeklyGoal = dashboard?.weeklyGoalMiles ?? 10;
  const weeklyMiles = dashboard?.weeklyMiles ?? 0;
  const weeklyProgress = weeklyGoal > 0 ? Math.round((weeklyMiles / weeklyGoal) * 100) : 0;
  const remainingMiles = Math.max(0, weeklyGoal - weeklyMiles);
  const userFirstName = user?.displayName?.split(" ", 2)[0] ?? "friend";

  return (
    <section className="home-page">
      <header className="home-page__intro">
        <div>
          <h1>{greeting}, {userFirstName}</h1>
          <p>Here&apos;s how {dogName} is doing this week.</p>
        </div>
        <Button aria-label="Notifications" className="home-page__notify" type="button" variant="icon">
          <MoochIcon name="bell" />
        </Button>
      </header>

      {isLoading && <p className="mooch-page__status">Loading your dogs...</p>}
      {error && <p className="mooch-page__status">Could not load your dogs right now.</p>}

      {hasDogs && selectedDog && (
        <>
          <div className="home-page__dog-selector" role="tablist" aria-label="Choose a dog profile">
            {dogs.map((dog) => {
              const isActive = dog.id === selectedDog.id;

              return (
                <button
                  aria-selected={isActive}
                  className={["home-dog-chip", isActive ? "home-dog-chip--active" : ""].filter(Boolean).join(" ")}
                  key={dog.id}
                  onClick={() => setSelectedDogId(dog.id)}
                  role="tab"
                  type="button"
                >
                  <span className="home-dog-chip__avatar">
                    {dog.avatarImage ? (
                      <img alt={dog.name} className="home-dog-chip__image" src={dog.avatarImage} />
                    ) : (
                      <span>{getDogInitials(dog.name)}</span>
                    )}
                  </span>
                  <span>{dog.name}</span>
                </button>
              );
            })}
          </div>

          {isDashboardLoading && <p className="mooch-page__status">Loading {selectedDog.name}&apos;s weekly summary...</p>}
          {dashboardError && <p className="mooch-page__status">Could not load the latest dog stats right now.</p>}

          {dashboard && (
            <>
              <div className="home-page__top-row">
                <article className="home-week-card">
                  <div className="home-week-card__hero">
                    <div className="home-week-card__avatar">
                      {selectedDog.avatarImage ? (
                        <img alt={selectedDog.name} className="home-week-card__image" src={selectedDog.avatarImage} />
                      ) : (
                        <span>{getDogInitials(selectedDog.name)}</span>
                      )}
                    </div>
                    <div className="home-week-card__copy">
                      <div className="home-week-card__title-row">
                        <h2>{dashboard.name}&apos;s week</h2>
                        <span className="home-week-card__percent">{weeklyProgress}%</span>
                      </div>
                      <p className="home-week-card__distance">{dashboard.weeklyMiles.toFixed(1)} / {dashboard.weeklyGoalMiles.toFixed(0)} miles</p>
                      <p className="home-week-card__remaining">{remainingMiles.toFixed(1)} miles to go</p>
                      <div className="home-week-card__progress" role="presentation">
                        <span className="home-week-card__progress-fill" style={{ width: `${Math.min(100, weeklyProgress)}%` }} />
                      </div>
                    </div>
                  </div>

                  <Button className="home-week-card__cta" type="button">
                    <MoochIcon name="walk" />
                    <span>Start walk</span>
                  </Button>
                </article>

                <section className="home-summary-grid" aria-label="Weekly dog statistics">
                  <article className="home-stat-card">
                    <div className="home-stat-card__icon"><MoochIcon name="map" /></div>
                    <h3>{dashboard.weeklyMiles.toFixed(1)}<span> mi</span></h3>
                    <p>Miles this week</p>
                  </article>
                  <article className="home-stat-card">
                    <div className="home-stat-card__icon"><MoochIcon name="walk" /></div>
                    <h3>{dashboard.weeklyAdventures}</h3>
                    <p>Walks</p>
                  </article>
                  <article className="home-stat-card">
                    <div className="home-stat-card__icon"><MoochIcon name="clock" /></div>
                    <h3>{formatOutsideTime(dashboard.weeklyOutsideMinutes)}</h3>
                    <p>Outside</p>
                  </article>
                  <article className="home-stat-card">
                    <div className="home-stat-card__icon"><MoochIcon name="ranks" /></div>
                    <h3>#{dashboard.packRank}</h3>
                    <p>Cincinnati rank</p>
                  </article>
                </section>
              </div>

              <section className="home-activity-panel">
                <div className="home-activity-panel__header">
                  <div>
                    <h2>Recent activity</h2>
                    <p>{dashboard.name}&apos;s latest activity</p>
                  </div>
                  <button className="home-activity-panel__action" type="button">
                    <span>View all</span>
                    <MoochIcon name="chevron-right" />
                  </button>
                </div>

                <div className="home-activity-list">
                  {dashboard.recentActivities.length === 0 && (
                    <EmptyState
                      message="Once your dog logs walks, runs, or hikes, their latest adventures will show up here."
                      title="No adventures yet"
                    />
                  )}

                  {dashboard.recentActivities.map((activity, index) => (
                    <article className="home-activity-row" key={activity.id}>
                      <div className="home-activity-row__index">{index + 1}</div>
                      <div className="home-activity-row__thumb">
                        <MoochIcon name="map" />
                      </div>
                      <div className="home-activity-row__body">
                        <h3>{activity.title}</h3>
                        <p>{formatActivityDate(activity.startedAt)} · {activity.distanceMiles.toFixed(1)} mi · {activity.durationMinutes} min</p>
                      </div>
                      <time className="home-activity-row__time" dateTime={activity.startedAt}>{formatClockTime(activity.startedAt)}</time>
                    </article>
                  ))}
                </div>
              </section>
            </>
          )}
        </>
      )}
    </section>
  );
}
