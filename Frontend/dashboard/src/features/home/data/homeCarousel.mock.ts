import type { HomeSlide } from '../model/home.types'
import hero1Image from '../../../assets/images/home/hero01.jpeg'
import hero2Image from '../../../assets/images/home/hero02.jpeg'
import hero3Image from '../../../assets/images/home/hero03.jpeg'

export const homeSlides: HomeSlide[] = [
  {
    id: 'certificate-hygiene',
    title: 'Understand Your Security at a Glance',
    description:
      'Quickly analyze your domain and identify potential security risks before they become critical.',
    imageUrl: hero1Image,
    imageAlt: 'Person working on laptop with phone near security dashboard',
  },
  {
    id: 'header-hardening',
    title: 'Comprehensive Security Analysis',
    description:
      'We check your TLS configuration, HTTP security headers, email protection, and domain reputation.',
    imageUrl: hero2Image,
    imageAlt: 'Code editor showing web security related source code',
  },
  {
    id: 'email-authentication',
    title: 'Detect Risks Before They Impact You',
    description:
      "Get a clear security score and actionable insights to improve your system's protection.",
    imageUrl: hero3Image,
    imageAlt: 'Digital lock visual representing secure email and identity',
  },
]
