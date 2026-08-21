import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { Navigate, useNavigate } from "react-router-dom";

import { useAuth } from "../features/auth/auth-context";
import { createDog, fetchDogs } from "../features/dogs/api";
import { CreateDogForm } from "../features/dogs/components/create-dog-form";
import { Badge } from "../shared/ui/badge";
import { Button } from "../shared/ui/button";

type PermissionState = "idle" | "granted" | "skipped";

export function OnboardingPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [locationPermission, setLocationPermission] = useState<PermissionState>("idle");
  const [fitnessPermission, setFitnessPermission] = useState<PermissionState>("idle");
  const [notificationsPermission, setNotificationsPermission] = useState<PermissionState>("idle");

  const { data: dogs = [], isLoading, error } = useQuery({
    queryKey: ["dogs"],
    queryFn: fetchDogs
  });

  const createDogMutation = useMutation({
    mutationFn: createDog,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["dogs"] });
    }
  });

  const hasDogs = dogs.length > 0;

  async function handleLocationPermission() {
    if (!navigator.geolocation) {
      setLocationPermission("skipped");
      return;
    }

    await new Promise<void>((resolve) => {
      navigator.geolocation.getCurrentPosition(
        () => {
          setLocationPermission("granted");
          resolve();
        },
        () => {
          setLocationPermission("skipped");
          resolve();
        },
        { enableHighAccuracy: true, timeout: 10000 }
      );
    });
  }

  async function handleNotificationPermission() {
    if (!("Notification" in window)) {
      setNotificationsPermission("skipped");
      return;
    }

    const result = await Notification.requestPermission();
    setNotificationsPermission(result === "granted" ? "granted" : "skipped");
  }

  const isReadyForHome = hasDogs;

  const permissionSummary = useMemo(() => {
    return [
      { label: "Location", value: locationPermission, required: true },
      { label: "Motion & fitness", value: fitnessPermission, required: false },
      { label: "Notifications", value: notificationsPermission, required: false }
    ];
  }, [fitnessPermission, locationPermission, notificationsPermission]);

  if (!isLoading && !error && hasDogs && locationPermission === "granted") {
    return <Navigate replace to="/app" />;
  }

  return (
    <section className="page">
      <div className="hero hero--mooch">
        <p className="eyebrow">Welcome to Mooch</p>
        <h1>Your dog is the athlete. We are just getting the team ready.</h1>
        <p className="hero__copy">
          We will start with your profile, add your first dog, then set up the permissions that make walk tracking feel effortless.
        </p>
      </div>

      <section className="onboarding-steps">
        <article className="panel onboarding-step">
          <div className="onboarding-step__title">
            <span className="onboarding-step__number">1</span>
            <div>
              <p className="eyebrow">Profile</p>
              <h2>Create your profile</h2>
            </div>
          </div>
          <p>
            You are signing in as <strong>{user?.displayName ?? "Walker"}</strong> with <strong>{user?.email ?? "your account"}</strong>.
            Your dog will be the star, but your human profile still anchors the account.
          </p>
        </article>

        <article className="panel onboarding-step">
          <div className="onboarding-step__title">
            <span className="onboarding-step__number">2</span>
            <div>
              <p className="eyebrow">Dog profile</p>
              <h2>Create your first dog</h2>
            </div>
          </div>
          <p className="page-header__copy">
            Name is the only must-have right now. Breed, birthday, weight, and photo help make home feel more personal.
          </p>

          <CreateDogForm
            ctaLabel={hasDogs ? "Add another dog" : "Save first dog"}
            helperText="Home area or ZIP is a good next schema addition. For this first route, let's get the pack in place and keep moving."
            isSaving={createDogMutation.isPending}
            onSubmit={createDogMutation.mutateAsync}
          />

          {isLoading && <p>Loading your dogs...</p>}
          {error && <p>Could not load your dogs right now.</p>}
          {hasDogs && (
            <div className="onboarding-pack-preview">
              <p className="eyebrow">Pack ready</p>
              <h3>{dogs.length} dog{dogs.length === 1 ? "" : "s"} added</h3>
              <p>Home can center on one dog or let you switch across the whole pack later.</p>
            </div>
          )}
        </article>

        <article className="panel onboarding-step">
          <div className="onboarding-step__title">
            <span className="onboarding-step__number">3</span>
            <div>
              <p className="eyebrow">Permissions</p>
              <h2>Set up tracking permissions</h2>
            </div>
          </div>
          <p className="page-header__copy">
            Location is the big one for tracking. Motion/fitness and notifications can stay light for now and get stronger later, especially on mobile.
          </p>

          <div className="permission-grid">
            <div className="permission-card">
              <h3>Location</h3>
              <p>Needed so Mooch can track the route and distance of a walk.</p>
              <Button onClick={() => void handleLocationPermission()} type="button" variant="secondary">
                {locationPermission === "granted" ? "Location enabled" : "Enable location"}
              </Button>
            </div>

            <div className="permission-card">
              <h3>Motion & fitness</h3>
              <p>Optional for now. We can use this later for smoother imports and activity context.</p>
              <Button onClick={() => setFitnessPermission("granted")} type="button" variant="secondary">
                {fitnessPermission === "granted" ? "Marked for later setup" : "Mark as planned"}
              </Button>
            </div>

            <div className="permission-card">
              <h3>Notifications</h3>
              <p>Optional, but handy for streak nudges, friend reactions, and reminders to get outside.</p>
              <Button onClick={() => void handleNotificationPermission()} type="button" variant="secondary">
                {notificationsPermission === "granted" ? "Notifications enabled" : "Enable notifications"}
              </Button>
            </div>
          </div>

          <div className="permission-summary">
            {permissionSummary.map((item) => (
              <Badge key={item.label} variant={item.value === "granted" ? "success" : item.required ? "gold" : "neutral"}>
                {item.label}: {item.value === "granted" ? "ready" : item.required ? "needed" : "optional"}
              </Badge>
            ))}
          </div>
        </article>
      </section>

      <section className="panel onboarding-finish">
        <div>
          <p className="eyebrow">Next</p>
          <h2>Head home</h2>
        </div>
        <p>
          Once at least one dog is set up, home can answer the two big questions: how is my dog doing, and what should I do next?
        </p>
        <Button
          disabled={!isReadyForHome}
          onClick={() => navigate("/app")}
          type="button"
        >
          {isReadyForHome ? "Go to home" : "Add a dog first"}
        </Button>
      </section>
    </section>
  );
}
