import { createBrowserRouter } from "react-router-dom";

import { AuthGuard } from "../features/auth/auth-guard";
import { CreateDogPage } from "../pages/create-dog-page";
import { LandingPage } from "../pages/landing-page";
import { DogsPage } from "../pages/dogs-page";
import { HomePage } from "../pages/home-page";
import { OnboardingPage } from "../pages/onboarding-page";
import { PackPage } from "../pages/pack-page";
import { RanksPage } from "../pages/ranks-page";
import { WalkPage } from "../pages/walk-page";
import { AppShell } from "../shared/layout/app-shell";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <LandingPage />
  },
  {
    path: "/app",
    element: (
      <AuthGuard>
        <AppShell />
      </AuthGuard>
    ),
    children: [
      {
        index: true,
        element: <HomePage />
      },
      {
        path: "onboarding",
        element: <OnboardingPage />
      },
      {
        path: "pack",
        element: <PackPage />
      },
      {
        path: "walk",
        element: <WalkPage />
      },
      {
        path: "ranks",
        element: <RanksPage />
      },
      {
        path: "dogs",
        element: <DogsPage />
      },
      {
        path: "dogs/new",
        element: <CreateDogPage />
      }
    ]
  }
]);
